namespace Reservations.Domain.Pricing.Rules;

/// <summary>+15% when there are more than 4 passengers (5 or 6).</summary>
public sealed class LargeGroupSurchargeRule : ISurchargeRule
{
    public PriceLine? Apply(PricingContext context, decimal baseAmount)
    {
        if (context.Passengers <= PricingConstants.LargeGroupThreshold)
            return null;

        var amount = baseAmount * PricingConstants.LargeGroupSurcharge;
        return new PriceLine("Large group surcharge (+15%)", amount);
    }
}
