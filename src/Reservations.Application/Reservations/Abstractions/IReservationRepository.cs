using Reservations.Domain.Entities;

namespace Reservations.Application.Reservations.Abstractions;

/// <summary>
/// Reservation persistence abstraction. The concrete implementation (in-memory, EF Core, etc.)
/// lives in the Infrastructure layer; the Application layer depends only on this interface.
/// It is asynchronous so the contract is not tied to a synchronous storage.
/// </summary>
public interface IReservationRepository
{
    Task<Reservation> AddAsync(Reservation reservation, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken ct = default);
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(Reservation reservation, CancellationToken ct = default);

    /// <summary>
    /// Checks the duplication rule: whether a reservation exists with the same combination of
    /// customer, origin, destination, date and service type (ignoring cancelled ones).
    /// </summary>
    Task<bool> ExistsDuplicateAsync(
        string customerName,
        string origin,
        string destination,
        DateTime date,
        Domain.Enums.ServiceType serviceType,
        CancellationToken ct = default);
}
