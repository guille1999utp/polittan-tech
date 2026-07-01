namespace Reservations.Domain.Pricing;

/// <summary>
/// A percentage surcharge/discount rule applied over the base subtotal.
/// Each rule is independent (Strategy pattern), which allows adding or removing
/// rules without modifying the calculation engine (Open/Closed principle).
/// </summary>
public interface ISurchargeRule
{
    /// <summary>
    /// Evaluates the rule. Returns the price line to apply, or <c>null</c> if the rule does not apply.
    /// </summary>
    /// <param name="context">Data of the reservation being priced.</param>
    /// <param name="baseAmount">Base subtotal (fare + passengers) the percentage is computed on.</param>
    PriceLine? Apply(PricingContext context, decimal baseAmount);
}
