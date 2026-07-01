using Reservations.Domain.Enums;

namespace Reservations.Domain.Pricing;

/// <summary>
/// Business parameters of the fare schedule, centralized to make them easy to tune and test.
/// Amounts are expressed in COP.
/// </summary>
public static class PricingConstants
{
    public const decimal StandardBaseFare = 50_000m;
    public const decimal PremiumBaseFare = 80_000m;
    public const decimal PricePerPassenger = 10_000m;

    public const decimal SameDaySurcharge = 0.20m;          // +20%
    public const decimal LargeGroupSurcharge = 0.15m;       // +15% when more than 4 passengers
    public const decimal PremiumLargeGroupSurcharge = 0.10m; // +10% when premium and more than 3 passengers
    public const decimal AdvanceBookingDiscount = 0.05m;    // -5% with 2+ days of anticipation

    public const int LargeGroupThreshold = 4;        // "more than 4" => 5 or 6
    public const int PremiumGroupThreshold = 3;      // "more than 3" => 4, 5 or 6
    public const int AdvanceBookingDays = 2;         // "2+ days of anticipation"

    public static decimal BaseFareFor(ServiceType serviceType) => serviceType switch
    {
        ServiceType.Standard => StandardBaseFare,
        ServiceType.Premium => PremiumBaseFare,
        _ => throw new ArgumentOutOfRangeException(nameof(serviceType), serviceType, "Unsupported service type.")
    };
}
