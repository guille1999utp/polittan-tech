using Reservations.Domain.Pricing;
using Reservations.Domain.Pricing.Rules;

namespace Reservations.UnitTests.TestHelpers;

/// <summary>Builds a pricing engine with the full set of rules, as in production.</summary>
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
