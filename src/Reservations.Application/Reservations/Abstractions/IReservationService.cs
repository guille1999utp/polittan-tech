using Reservations.Application.Reservations.Dtos;
using Reservations.Domain.Common;

namespace Reservations.Application.Reservations.Abstractions;

/// <summary>Casos de uso de reservas expuestos a la capa de API.</summary>
public interface IReservationService
{
    /// <summary>Crea una reserva. Falla (Conflict) si viola la regla de duplicidad.</summary>
    Task<Result<ReservationResponse>> CreateAsync(CreateReservationRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<ReservationResponse>> GetAllAsync(CancellationToken ct = default);

    Task<Result<ReservationResponse>> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<Result<ReservationResponse>> ConfirmAsync(Guid id, CancellationToken ct = default);

    Task<Result<ReservationResponse>> CancelAsync(Guid id, CancellationToken ct = default);
}
