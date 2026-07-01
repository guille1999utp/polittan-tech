namespace Reservations.Domain.Pricing;

/// <summary>
/// Motor de tarifación. Calcula el subtotal base (tarifa del servicio + costo por pasajero)
/// y luego aplica cada regla de recargo/descuento sobre ese subtotal.
///
/// Supuesto de negocio (documentado en el README): los porcentajes son <b>aditivos</b> y se
/// calculan siempre sobre el subtotal base, no compuestos entre sí. Esto hace el cálculo
/// determinista, explicable y fácil de auditar línea por línea.
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
            new($"Tarifa base {context.ServiceType}", baseFare),
            new($"Pasajeros ({context.Passengers} x {PricingConstants.PricePerPassenger:N0})", passengersCost)
        };

        foreach (var rule in _rules)
        {
            var line = rule.Apply(context, baseAmount);
            if (line is not null)
                lines.Add(line);
        }

        var total = lines.Sum(l => l.Amount);

        // Se redondea a peso (COP no maneja decimales en la práctica).
        total = Math.Round(total, 0, MidpointRounding.AwayFromZero);

        return new PriceQuote(total, lines);
    }
}
