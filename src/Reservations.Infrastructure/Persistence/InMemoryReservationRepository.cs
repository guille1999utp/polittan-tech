using System.Collections.Concurrent;
using Reservations.Application.Reservations.Abstractions;
using Reservations.Domain.Entities;
using Reservations.Domain.Enums;

namespace Reservations.Infrastructure.Persistence;

/// <summary>
/// Almacenamiento en memoria (thread-safe) de reservas. El enunciado no requiere base de datos.
/// Se registra como Singleton para que los datos persistan durante la vida del proceso.
/// Sustituir por una implementación real (EF Core, Dapper) no impacta al resto de capas.
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
        // La entidad es mutable en su estado y ya es la misma referencia almacenada;
        // se reasigna para dejar explícita la intención de persistir el cambio.
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
