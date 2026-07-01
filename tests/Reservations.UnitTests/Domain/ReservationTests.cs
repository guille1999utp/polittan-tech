using FluentAssertions;
using Reservations.Domain.Common;
using Reservations.Domain.Entities;
using Reservations.Domain.Enums;
using Reservations.Domain.Pricing;
using Xunit;

namespace Reservations.UnitTests.Domain;

public class ReservationTests
{
    private static readonly DateTime Now = new(2026, 1, 10, 8, 0, 0);

    private static Reservation NewReservation() => Reservation.Create(
        customerName: " Juan Pérez ",
        origin: " Bogotá ",
        destination: " Aeropuerto El Dorado ",
        date: Now.AddDays(5),
        passengers: 3,
        serviceType: ServiceType.Standard,
        quote: new PriceQuote(76_000m, new[] { new PriceLine("Base fare Standard", 50_000m) }),
        now: Now);

    [Fact]
    public void Create_SetsInitialStatusToCreated_AndTrimsText()
    {
        var reservation = NewReservation();

        reservation.Status.Should().Be(ReservationStatus.Created);
        reservation.CustomerName.Should().Be("Juan Pérez");
        reservation.Origin.Should().Be("Bogotá");
        reservation.Price.Should().Be(76_000m);
    }

    [Fact]
    public void Confirm_FromCreated_Succeeds()
    {
        var reservation = NewReservation();

        var result = reservation.Confirm(Now);

        result.IsSuccess.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Confirmed);
    }

    [Fact]
    public void Confirm_WhenCancelled_Fails()
    {
        var reservation = NewReservation();
        reservation.Cancel(Now);

        var result = reservation.Confirm(Now);

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.Conflict);
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Confirm_WhenAlreadyConfirmed_Fails()
    {
        var reservation = NewReservation();
        reservation.Confirm(Now);

        var result = reservation.Confirm(Now);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("reservation.already_confirmed");
    }

    [Fact]
    public void Cancel_FromConfirmed_Succeeds()
    {
        var reservation = NewReservation();
        reservation.Confirm(Now);

        var result = reservation.Cancel(Now);

        result.IsSuccess.Should().BeTrue();
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_Fails()
    {
        var reservation = NewReservation();
        reservation.Cancel(Now);

        var result = reservation.Cancel(Now);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("reservation.already_cancelled");
    }
}
