using System.Collections.Concurrent;
using Reservations.Application.Reservations.Abstractions;
using Reservations.Domain.Entities;
using Reservations.Domain.Enums;

namespace Reservations.Infrastructure.Persistence;

/// <summary>
/// Thread-safe in-memory reservation storage. The exercise does not require a database.
/// It is registered as a Singleton so data persists for the lifetime of the process.
/// Replacing it with a real implementation (EF Core, Dapper) does not impact the other layers.
/// </summary>
public sealed class InMemoryReservationRepository : IReservationRepository
{
    private readonly ConcurrentDictionary<Guid, Reservation> _store = new();

    public Task<Reservation> AddAsync(Reservation reservation, CancellationToken ct = default)
    {
        _store[reservation.Id] = reservation;
        return Task.FromResult(reservation);
    }

    public Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken ct = default)
    {
        IReadOnlyList<Reservation> result = _store.Values
            .OrderByDescending(r => r.CreatedAt)
            .ToList();
        return Task.FromResult(result);
    }

    public Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var reservation);
        return Task.FromResult(reservation);
    }

    public Task UpdateAsync(Reservation reservation, CancellationToken ct = default)
    {
        // The entity is mutable in its state and is already the same stored reference;
        // it is reassigned to make the intent to persist the change explicit.
        _store[reservation.Id] = reservation;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsDuplicateAsync(
        string customerName,
        string origin,
        string destination,
        DateTime date,
        ServiceType serviceType,
        CancellationToken ct = default)
    {
        var exists = _store.Values.Any(r =>
            r.Status != ReservationStatus.Cancelled &&
            string.Equals(r.CustomerName, customerName, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.Origin, origin, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.Destination, destination, StringComparison.OrdinalIgnoreCase) &&
            r.Date == date &&
            r.ServiceType == serviceType);

        return Task.FromResult(exists);
    }
}
