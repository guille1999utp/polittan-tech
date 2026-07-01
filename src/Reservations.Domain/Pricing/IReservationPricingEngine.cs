namespace Reservations.Domain.Pricing;

/// <summary>Computes the price of a reservation from its context.</summary>
public interface IReservationPricingEngine
{
    PriceQuote Quote(PricingContext context);
}
