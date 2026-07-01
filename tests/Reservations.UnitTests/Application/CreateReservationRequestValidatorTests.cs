using FluentAssertions;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Time.Testing;
using Reservations.Application.Reservations.Dtos;
using Reservations.Application.Reservations.Validation;
using Xunit;

namespace Reservations.UnitTests.Application;

public class CreateReservationRequestValidatorTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 10, 8, 0, 0, TimeSpan.Zero);
    private readonly CreateReservationRequestValidator _validator =
        new(new FakeTimeProvider(Now));

    private static CreateReservationRequest ValidRequest() => new()
    {
        CustomerName = "Juan Pérez",
        Origin = "Bogotá",
        Destination = "Aeropuerto El Dorado",
        Date = Now.DateTime.AddDays(3),
        Passengers = 3,
        ServiceType = "standard"
    };

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void MissingCustomerName_Fails(string? name)
    {
        var request = ValidRequest() with { CustomerName = name };
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(7)]
    public void PassengersOutOfRange_Fails(int passengers)
    {
        var request = ValidRequest() with { Passengers = passengers };
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Passengers);
    }

    [Fact]
    public void PastDate_Fails()
    {
        var request = ValidRequest() with { Date = Now.DateTime.AddDays(-1) };
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void SameOriginAndDestination_Fails()
    {
        // Distinto casing y espacios: debe detectarse como igual y fallar.
        var request = ValidRequest() with { Origin = "Bogotá", Destination = " bogotá " };
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.Destination);
    }

    [Fact]
    public void MissingDate_FailsGracefully_WithoutThrowing()
    {
        // Regresión: un payload sin 'date' no debe lanzar excepción, sino fallar la validación.
        var request = ValidRequest() with { Date = null };

        var act = () => _validator.TestValidate(request);

        act.Should().NotThrow();
        act().ShouldHaveValidationErrorFor(x => x.Date);
    }

    [Fact]
    public void MissingMultipleFields_ReportsAllErrors_WithoutThrowing()
    {
        // Payload como el reportado: sin 'date' y con pasajeros fuera de rango.
        var request = new CreateReservationRequest
        {
            CustomerName = "Carlos Ruiz",
            Origin = "Cali",
            Destination = "Terminal Norte",
            Passengers = 8,
            ServiceType = "standard"
            // Date ausente
        };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Date);
        result.ShouldHaveValidationErrorFor(x => x.Passengers);
    }

    [Fact]
    public void InvalidServiceType_Fails()
    {
        var request = ValidRequest() with { ServiceType = "luxury" };
        _validator.TestValidate(request).ShouldHaveValidationErrorFor(x => x.ServiceType);
    }

    [Fact]
    public void ServiceType_IsCaseInsensitive()
    {
        var request = ValidRequest() with { ServiceType = "PREMIUM" };
        _validator.TestValidate(request).ShouldNotHaveValidationErrorFor(x => x.ServiceType);
    }
}
