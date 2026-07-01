namespace Reservations.Domain.Pricing.Rules;

/// <summary>+20% when the reservation is for the same day.</summary>
public sealed class SameDaySurchargeRule : ISurchargeRule
{
    public PriceLine? Apply(PricingContext context, decimal baseAmount)
    {
        if (!context.IsSameDay)
            return null;

        var amount = baseAmount * PricingConstants.SameDaySurcharge;
        return new PriceLine("Same-day surcharge (+20%)", amount);
    }
}
