namespace Reservations.Domain.Pricing.Rules;

/// <summary>+15% cuando hay más de 4 pasajeros (5 o 6).</summary>
public sealed class LargeGroupSurchargeRule : ISurchargeRule
{
    public PriceLine? Apply(PricingContext context, decimal baseAmount)
    {
        if (context.Passengers <= PricingConstants.LargeGroupThreshold)
            return null;

        var amount = baseAmount * PricingConstants.LargeGroupSurcharge;
        return new PriceLine("Recargo grupo grande (+15%)", amount);
    }
}
