namespace Reservations.Domain.Enums;

/// <summary>
/// Ciclo de vida de una reserva.
/// Transiciones válidas: Created -> Confirmed, Created -> Cancelled, Confirmed -> Cancelled.
/// </summary>
public enum ReservationStatus
{
    Created = 0,
    Confirmed = 1,
    Cancelled = 2
}
