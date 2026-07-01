namespace Reservations.Domain.Pricing.Rules;

/// <summary>+20% cuando la reserva es para el mismo día.</summary>
public sealed class SameDaySurchargeRule : ISurchargeRule
{
    public PriceLine? Apply(PricingContext context, decimal baseAmount)
    {
        if (!context.IsSameDay)
            return null;

        var amount = baseAmount * PricingConstants.SameDaySurcharge;
        return new PriceLine("Recargo mismo día (+20%)", amount);
    }
}
