namespace Reservations.Domain.Pricing;

/// <summary>
/// Regla de recargo/descuento porcentual aplicada sobre el subtotal base.
/// Cada regla es independiente (patrón Strategy), lo que permite agregar o quitar
/// reglas sin modificar el motor de cálculo (principio Open/Closed).
/// </summary>
public interface ISurchargeRule
{
    /// <summary>
    /// Evalúa la regla. Devuelve la línea de precio a aplicar, o <c>null</c> si la regla no aplica.
    /// </summary>
    /// <param name="context">Datos de la reserva a tarifar.</param>
    /// <param name="baseAmount">Subtotal base (tarifa + pasajeros) sobre el cual se calcula el porcentaje.</param>
    PriceLine? Apply(PricingContext context, decimal baseAmount);
}
