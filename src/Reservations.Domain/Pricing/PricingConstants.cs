using Reservations.Domain.Enums;

namespace Reservations.Domain.Pricing;

/// <summary>
/// Parámetros de negocio del tarifario, centralizados para facilitar su ajuste y prueba.
/// Los montos están expresados en COP.
/// </summary>
public static class PricingConstants
{
    public const decimal StandardBaseFare = 50_000m;
    public const decimal PremiumBaseFare = 80_000m;
    public const decimal PricePerPassenger = 10_000m;

    public const decimal SameDaySurcharge = 0.20m;          // +20%
    public const decimal LargeGroupSurcharge = 0.15m;       // +15% si > 4 pasajeros
    public const decimal PremiumLargeGroupSurcharge = 0.10m; // +10% si premium y > 3 pasajeros
    public const decimal AdvanceBookingDiscount = 0.05m;    // -5% con 2+ días de anticipación

    public const int LargeGroupThreshold = 4;        // "más de 4" => 5 o 6
    public const int PremiumGroupThreshold = 3;      // "más de 3" => 4, 5 o 6
    public const int AdvanceBookingDays = 2;         // "2+ días de anticipación"

    public static decimal BaseFareFor(ServiceType serviceType) => serviceType switch
    {
        ServiceType.Standard => StandardBaseFare,
        ServiceType.Premium => PremiumBaseFare,
        _ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, "Tipo de servicio no soportado.")
    };
}
