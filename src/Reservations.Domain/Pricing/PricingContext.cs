using Reservations.Domain.Enums;

namespace Reservations.Domain.Pricing;

/// <summary>
/// Data required to price a reservation. Includes <paramref name="Now"/> explicitly so that
/// the calculation is deterministic and easy to test (it does not rely on an internal DateTime.Now).
/// </summary>
public sealed record PricingContext(
    ServiceType ServiceType,
    int Passengers,
    DateTime ReservationDate,
    DateTime Now)
{
    /// <summary>The reservation is for the same calendar day it is being quoted.</summary>
    public bool IsSameDay => ReservationDate.Date == Now.Date;

    /// <summary>Whole days of anticipation between today and the reservation date.</summary>
    public int DaysInAdvance => (ReservationDate.Date - Now.Date).Days;
}
