using FluentAssertions;
using Reservations.Domain.Enums;
using Reservations.Domain.Pricing;
using Reservations.UnitTests.TestHelpers;
using Xunit;

namespace Reservations.UnitTests.Pricing;

public class ReservationPricingEngineTests
{
    private readonly ReservationPricingEngine _engine = PricingEngineFactory.CreateWithAllRules();

    // Referencia fija para pruebas deterministas.
    private static readonly DateTime Now = new(2026, 1, 10, 8, 0, 0);

    [Fact]
    public void Standard_3Passengers_2DaysAdvance_AppliesOnlyAdvanceDiscount()
    {
        // base = 50.000 + 3*10.000 = 80.000; descuento -5% = -4.000 => 76.000
        var context = new PricingContext(ServiceType.Standard, 3, Now.AddDays(2), Now);

        var quote = _engine.Quote(context);

        quote.Total.Should().Be(76_000m);
    }

    [Fact]
    public void Standard_SameDay_AppliesSameDaySurcharge()
    {
        // base = 80.000; +20% = 16.000 => 96.000 (sin descuento por anticipación)
        var context = new PricingContext(ServiceType.Standard, 3, Now.AddHours(6), Now);

        var quote = _engine.Quote(context);

        quote.Total.Should().Be(96_000m);
    }

    [Fact]
    public void Premium_5Passengers_2DaysAdvance_AppliesLargeGroupPremiumAndDiscount()
    {
        // base = 80.000 + 5*10.000 = 130.000
        // +15% (grupo>4) = 19.500 ; +10% (premium>3) = 13.000 ; -5% = -6.500 => 156.000
        var context = new PricingContext(ServiceType.Premium, 5, Now.AddDays(2), Now);

        var quote = _engine.Quote(context);

        quote.Total.Should().Be(156_000m);
    }

    [Fact]
    public void Premium_4Passengers_SameDay_AppliesSameDayAndPremiumSurcharge()
    {
        // base = 80.000 + 40.000 = 120.000 ; +20% = 24.000 ; +10% (premium>3) = 12.000 => 156.000
        // grupo grande NO aplica (4 no es > 4)
        var context = new PricingContext(ServiceType.Premium, 4, Now.AddHours(3), Now);

        var quote = _engine.Quote(context);

        quote.Total.Should().Be(156_000m);
    }

    [Fact]
    public void Premium_6Passengers_SameDay_AppliesAllSurcharges()
    {
        // base = 80.000 + 60.000 = 140.000 ; +20% = 28.000 ; +15% = 21.000 ; +10% = 14.000 => 203.000
        var context = new PricingContext(ServiceType.Premium, 6, Now.AddHours(1), Now);

        var quote = _engine.Quote(context);

        quote.Total.Should().Be(203_000m);
    }

    [Fact]
    public void OneDayAdvance_AppliesNoSurchargeNorDiscount()
    {
        // 1 día: ni mismo día ni 2+ de anticipación. base standard 1 pax = 60.000
        var context = new PricingContext(ServiceType.Standard, 1, Now.AddDays(1), Now);

        var quote = _engine.Quote(context);

        quote.Total.Should().Be(60_000m);
    }

    [Fact]
    public void Quote_IncludesBreakdown_ThatSumsToTotal()
    {
        var context = new PricingContext(ServiceType.Premium, 5, Now.AddDays(2), Now);

        var quote = _engine.Quote(context);

        quote.Lines.Should().Contain(l => l.Concept.Contains("Tarifa base"));
        quote.Lines.Sum(l => l.Amount).Should().Be(quote.Total);
    }
}
