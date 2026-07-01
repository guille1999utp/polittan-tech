using FluentAssertions;
using FluentValidation;
using Microsoft.Extensions.Time.Testing;
using Reservations.Application.Reservations;
using Reservations.Application.Reservations.Dtos;
using Reservations.Application.Reservations.Validation;
using Reservations.Domain.Common;
using Reservations.Infrastructure.Persistence;
using Reservations.UnitTests.TestHelpers;
using Xunit;

namespace Reservations.UnitTests.Application;

public class ReservationServiceTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 10, 8, 0, 0, TimeSpan.Zero);

    private readonly ReservationService _service;

    public ReservationServiceTests()
    {
        var timeProvider = new FakeTimeProvider(Now);
        var repository = new InMemoryReservationRepository();
        var engine = PricingEngineFactory.CreateWithAllRules();
        var validator = new CreateReservationRequestValidator(timeProvider);

        _service = new ReservationService(repository, engine, validator, timeProvider);
    }

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
    public async Task CreateAsync_ValidRequest_CreatesReservationWithComputedPrice()
    {
        var result = await _service.CreateAsync(ValidRequest());

        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Created");
        result.Value.ServiceType.Should().Be("Standard");
        // base 50.000 + 3*10.000 = 80.000, descuento -5% por 3 días de anticipación => 76.000
        result.Value.Price.Should().Be(76_000m);
    }

    [Fact]
    public async Task CreateAsync_InvalidRequest_ThrowsValidationException()
    {
        var invalid = ValidRequest() with { Passengers = 10 };

        var act = () => _service.CreateAsync(invalid);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_DuplicateReservation_ReturnsConflict()
    {
        await _service.CreateAsync(ValidRequest());

        var second = await _service.CreateAsync(ValidRequest());

        second.IsFailure.Should().BeTrue();
        second.Error!.Type.Should().Be(ErrorType.Conflict);
        second.Error!.Code.Should().Be("reservation.duplicate");
    }

    [Fact]
    public async Task CreateAsync_DuplicateOfCancelledReservation_IsAllowed()
    {
        var first = await _service.CreateAsync(ValidRequest());
        await _service.CancelAsync(first.Value.Id);

        var second = await _service.CreateAsync(ValidRequest());

        second.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNotFound()
    {
        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task ConfirmAsync_ThenConfirmAgain_ReturnsConflict()
    {
        var created = await _service.CreateAsync(ValidRequest());

        var confirmed = await _service.ConfirmAsync(created.Value.Id);
        var reconfirmed = await _service.ConfirmAsync(created.Value.Id);

        confirmed.IsSuccess.Should().BeTrue();
        confirmed.Value.Status.Should().Be("Confirmed");
        reconfirmed.IsFailure.Should().BeTrue();
        reconfirmed.Error!.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public async Task CancelAsync_ChangesStatusToCancelled()
    {
        var created = await _service.CreateAsync(ValidRequest());

        var cancelled = await _service.CancelAsync(created.Value.Id);

        cancelled.IsSuccess.Should().BeTrue();
        cancelled.Value.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCreatedReservations()
    {
        await _service.CreateAsync(ValidRequest());
        await _service.CreateAsync(ValidRequest() with { Destination = "Terminal Norte" });

        var all = await _service.GetAllAsync();

        all.Should().HaveCount(2);
    }
}
