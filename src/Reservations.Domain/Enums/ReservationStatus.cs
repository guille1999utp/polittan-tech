namespace Reservations.Domain.Enums;

/// <summary>
/// Reservation lifecycle.
/// Valid transitions: Created -> Confirmed, Created -> Cancelled, Confirmed -> Cancelled.
/// </summary>
public enum ReservationStatus
{
    Created = 0,
    Confirmed = 1,
    Cancelled = 2
}
