namespace Reservations.Domain.Pricing;

/// <summary>Calcula el precio de una reserva a partir de su contexto.</summary>
public interface IReservationPricingEngine
{
    PriceQuote Quote(PricingContext context);
}
