namespace Reservations.Domain.Pricing.Rules;

/// <summary>-5% when the reservation is made with 2 or more days of anticipation.</summary>
public sealed class AdvanceBookingDiscountRule : ISurchargeRule
{
    public PriceLine? Apply(PricingContext context, decimal baseAmount)
    {
        if (context.DaysInAdvance < PricingConstants.AdvanceBookingDays)
            return null;

        var amount = -(baseAmount * PricingConstants.AdvanceBookingDiscount);
        return new PriceLine("Advance booking discount (-5%)", amount);
    }
}
