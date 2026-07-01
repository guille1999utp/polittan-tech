using Reservations.Domain.Enums;

namespace Reservations.Domain.Pricing.Rules;

/// <summary>+10% adicional cuando el servicio es Premium y hay más de 3 pasajeros (4, 5 o 6).</summary>
public sealed class PremiumLargeGroupSurchargeRule : ISurchargeRule
{
    public PriceLine? Apply(PricingContext context, decimal baseAmount)
    {
        if (context.ServiceType != ServiceType.Premium || context.Passengers <= PricingConstants.PremiumGroupThreshold)
            return null;

        var amount = baseAmount * PricingConstants.PremiumLargeGroupSurcharge;
        return new PriceLine("Recargo Premium grupo grande (+10%)", amount);
    }
}
