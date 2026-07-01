namespace Reservations.Domain.Pricing;

/// <summary>
/// A single line of the price breakdown (base fare or surcharge/discount).
/// Lets the customer understand why the price is what it is.
/// </summary>
/// <param name="Concept">Human-readable description of the concept (e.g. "Same-day surcharge (+20%)").</param>
/// <param name="Amount">Amount in COP; positive for surcharges, negative for discounts.</param>
public sealed record PriceLine(string Concept, decimal Amount);

/// <summary>
/// Result of the price calculation: total plus its detailed breakdown.
/// </summary>
public sealed record PriceQuote(decimal Total, IReadOnlyList<PriceLine> Lines);
