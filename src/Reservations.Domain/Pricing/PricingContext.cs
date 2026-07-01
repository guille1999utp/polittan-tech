using Reservations.Domain.Enums;

namespace Reservations.Domain.Pricing;

/// <summary>
/// Datos necesarios para tarifar una reserva. Incluye <paramref name="Now"/> de forma explícita
/// para que el cálculo sea determinista y fácilmente testeable (no depende de DateTime.Now interno).
/// </summary>
public sealed record PricingContext(
    ServiceType ServiceType,
    int Passengers,
    DateTime ReservationDate,
    DateTime Now)
{
    /// <summary>La reserva es para el mismo día calendario en que se cotiza.</summary>
    public bool IsSameDay => ReservationDate.Date == Now.Date;

    /// <summary>Días completos de anticipación entre hoy y la fecha de la reserva.</summary>
    public int DaysInAdvance => (ReservationDate.Date - Now.Date).Days;
}
