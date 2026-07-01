namespace Reservations.Domain.Pricing.Rules;

/// <summary>-5% cuando la reserva se hace con 2 o más días de anticipación.</summary>
public sealed class AdvanceBookingDiscountRule : ISurchargeRule
{
    public PriceLine? Apply(PricingContext context, decimal baseAmount)
    {
        if (context.DaysInAdvance < PricingConstants.AdvanceBookingDays)
            return null;

        var amount = -(baseAmount * PricingConstants.AdvanceBookingDiscount);
        return new PriceLine("Descuento por anticipación (-5%)", amount);
    }
}
