using Reservations.Domain.Pricing;
using Reservations.Domain.Pricing.Rules;

namespace Reservations.UnitTests.TestHelpers;

/// <summary>Construye un motor de precios con el conjunto completo de reglas, tal como en producción.</summary>
public static class PricingEngineFactory
{
    public static ReservationPricingEngine CreateWithAllRules() => new(new ISurchargeRule[]
    {
        new SameDaySurchargeRule(),
        new LargeGroupSurchargeRule(),
        new PremiumLargeGroupSurchargeRule(),
        new AdvanceBookingDiscountRule()
    });
}
