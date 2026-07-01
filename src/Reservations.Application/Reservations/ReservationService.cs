using FluentValidation;
using Reservations.Application.Reservations.Abstractions;
using Reservations.Application.Reservations.Dtos;
using Reservations.Domain.Common;
using Reservations.Domain.Entities;
using Reservations.Domain.Pricing;

namespace Reservations.Application.Reservations;

/// <summary>
/// Orchestrates the reservation use cases: validates the input, applies the duplication rule,
/// computes the price with the pricing engine and coordinates persistence.
/// </summary>
public sealed class ReservationService : IReservationService
{
    private readonly IReservationRepository _repository;
    private readonly IReservationPricingEngine _pricingEngine;
    private readonly IValidator<CreateReservationRequest> _validator;
    private readonly TimeProvider _timeProvider;

    public ReservationService(
        IReservationRepository repository,
        IReservationPricingEngine pricingEngine,
        IValidator<CreateReservationRequest> validator,
        TimeProvider timeProvider)
    {
        _repository = repository;
        _pricingEngine = pricingEngine;
        _validator = validator;
        _timeProvider = timeProvider;
    }

    public async Task<Result<ReservationResponse>> CreateAsync(CreateReservationRequest request, CancellationToken ct = default)
    {
        // Input validation: throws ValidationException which the middleware turns into a 400 ProblemDetails.
        await _validator.ValidateAndThrowAsync(request, ct);

        // From here on the data is already valid.
        ServiceTypeParser.TryParse(request.ServiceType, out var serviceType);
        var customerName = request.CustomerName!.Trim();
        var origin = request.Origin!.Trim();
        var destination = request.Destination!.Trim();
        var date = request.Date!.Value;
        var passengers = request.Passengers!.Value;

        // Duplication rule.
        var isDuplicate = await _repository.ExistsDuplicateAsync(customerName, origin, destination, date, serviceType, ct);
        if (isDuplicate)
        {
            return Result.Failure<ReservationResponse>(Error.Conflict(
                "reservation.duplicate",
                "A reservation with the same customer, origin, destination, date and service type already exists."));
        }

        var now = _timeProvider.GetLocalNow().DateTime;
        var quote = _pricingEngine.Quote(new PricingContext(serviceType, passengers, date, now));

        var reservation = Reservation.Create(customerName, origin, destination, date, passengers, serviceType, quote, now);
        await _repository.AddAsync(reservation, ct);

        return Result.Success(ReservationResponse.FromEntity(reservation));
    }

    public async Task<IReadOnlyList<ReservationResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var reservations = await _repository.GetAllAsync(ct);
        return reservations.Select(ReservationResponse.FromEntity).ToList();
    }

    public async Task<Result<ReservationResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var reservation = await _repository.GetByIdAsync(id, ct);
        return reservation is null
            ? NotFound(id)
            : Result.Success(ReservationResponse.FromEntity(reservation));
    }

    public async Task<Result<ReservationResponse>> ConfirmAsync(Guid id, CancellationToken ct = default)
    {
        var reservation = await _repository.GetByIdAsync(id, ct);
        if (reservation is null)
            return NotFound(id);

        var result = reservation.Confirm(_timeProvider.GetLocalNow().DateTime);
        if (result.IsFailure)
            return Result.Failure<ReservationResponse>(result.Error!);

        await _repository.UpdateAsync(reservation, ct);
        return Result.Success(ReservationResponse.FromEntity(reservation));
    }

    public async Task<Result<ReservationResponse>> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var reservation = await _repository.GetByIdAsync(id, ct);
        if (reservation is null)
            return NotFound(id);

        var result = reservation.Cancel(_timeProvider.GetLocalNow().DateTime);
        if (result.IsFailure)
            return Result.Failure<ReservationResponse>(result.Error!);

        await _repository.UpdateAsync(reservation, ct);
        return Result.Success(ReservationResponse.FromEntity(reservation));
    }

    private static Result<ReservationResponse> NotFound(Guid id) =>
        Result.Failure<ReservationResponse>(Error.NotFound(
            "reservation.not_found",
            $"Reservation with id '{id}' was not found."));
}
