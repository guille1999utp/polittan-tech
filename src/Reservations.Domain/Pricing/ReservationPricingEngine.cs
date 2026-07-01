namespace Reservations.Domain.Pricing;

/// <summary>
/// Pricing engine. Computes the base subtotal (service fare + per-passenger cost) and then
/// applies each surcharge/discount rule over that subtotal.
///
/// Business assumption (documented in the README): percentages are <b>additive</b> and are
/// always computed over the base subtotal, not compounded with each other. This makes the
/// calculation deterministic, explainable and easy to audit line by line.
/// </summary>
public sealed class ReservationPricingEngine : IReservationPricingEngine
{
    private readonly IReadOnlyList<ISurchargeRule> _rules;

    public ReservationPricingEngine(IEnumerable<ISurchargeRule> rules)
    {
        _rules = rules.ToList();
    }

    public PriceQuote Quote(PricingContext context)
    {
        var baseFare = PricingConstants.BaseFareFor(context.ServiceType);
        var passengersCost = PricingConstants.PricePerPassenger * context.Passengers;
        var baseAmount = baseFare + passengersCost;

        var lines = new List<PriceLine>
        {
            new($"Base fare {context.ServiceType}", baseFare),
            new($"Passengers ({context.Passengers} x {PricingConstants.PricePerPassenger:N0})", passengersCost)
        };

        foreach (var rule in _rules)
        {
            var line = rule.Apply(context, baseAmount);
            if (line is not null)
                lines.Add(line);
        }

        var total = lines.Sum(l => l.Amount);

        // Rounded to whole pesos (COP has no decimal cents in practice).
        total = Math.Round(total, 0, MidpointRounding.AwayFromZero);

        return new PriceQuote(total, lines);
    }
}
