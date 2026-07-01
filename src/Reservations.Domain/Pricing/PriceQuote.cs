namespace Reservations.Domain.Pricing;

/// <summary>
/// Una línea del desglose del precio (base o recargo/descuento).
/// Permite explicar al cliente por qué el precio es el que es.
/// </summary>
/// <param name="Concept">Descripción legible del concepto (p. ej. "Recargo mismo día (+20%)").</param>
/// <param name="Amount">Monto en COP; positivo para recargos, negativo para descuentos.</param>
public sealed record PriceLine(string Concept, decimal Amount);

/// <summary>
/// Resultado del cálculo de precio: total y su desglose detallado.
/// </summary>
public sealed record PriceQuote(decimal Total, IReadOnlyList<PriceLine> Lines);
