using Reservations.Application.Reservations.Dtos;
using Reservations.Domain.Common;

namespace Reservations.Application.Reservations.Abstractions;

/// <summary>Reservation use cases exposed to the API layer.</summary>
public interface IReservationService
{
    /// <summary>Creates a reservation. Fails (Conflict) if it violates the duplication rule.</summary>
    Task<Result<ReservationResponse>> CreateAsync(CreateReservationRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<ReservationResponse>> GetAllAsync(CancellationToken ct = default);

    Task<Result<ReservationResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Result<ReservationResponse>> ConfirmAsync(Guid id, CancellationToken ct = default);

    Task<Result<ReservationResponse>> CancelAsync(Guid id, CancellationToken ct = default);
}
