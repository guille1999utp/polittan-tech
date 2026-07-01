using Reservations.Domain.Enums;

namespace Reservations.Domain.Pricing.Rules;

/// <summary>Additional +10% when the service is Premium and there are more than 3 passengers (4, 5 or 6).</summary>
public sealed class PremiumLargeGroupSurchargeRule : ISurchargeRule
{
    public PriceLine? Apply(PricingContext context, decimal baseAmount)
    {
        if (context.ServiceType != ServiceType.Premium || context.Passengers <= PricingConstants.PremiumGroupThreshold)
            return null;

        var amount = baseAmount * PricingConstants.PremiumLargeGroupSurcharge;
        return new PriceLine("Premium large group surcharge (+10%)", amount);
    }
}
