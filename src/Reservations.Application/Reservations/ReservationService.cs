using FluentValidation;
using Reservations.Application.Reservations.Abstractions;
using Reservations.Application.Reservations.Dtos;
using Reservations.Domain.Common;
using Reservations.Domain.Entities;
using Reservations.Domain.Pricing;

namespace Reservations.Application.Reservations;

/// <summary>
/// Orquesta los casos de uso de reservas: valida el input, aplica la regla de duplicidad,
/// calcula el precio con el motor de tarifación y coordina la persistencia.
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
        // Validación de input: lanza ValidationException que el middleware convierte en 400 ProblemDetails.
        await _validator.ValidateAndThrowAsync(request, ct);

        // A partir de aquí los datos ya son válidos.
        ServiceTypeParser.TryParse(request.ServiceType, out var serviceType);
        var customerName = request.CustomerName!.Trim();
        var origin = request.Origin!.Trim();
        var destination = request.Destination!.Trim();
        var date = request.Date!.Value;
        var passengers = request.Passengers!.Value;

        // Regla de duplicidad.
        var isDuplicate = await _repository.ExistsDuplicateAsync(customerName, origin, destination, date, serviceType, ct);
        if (isDuplicate)
        {
            return Result.Failure<ReservationResponse>(Error.Conflict(
                "reservation.duplicate",
                "Ya existe una reserva idéntica (mismo cliente, origen, destino, fecha y tipo de servicio)."));
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
            $"No se encontró la reserva con id '{id}'."));
}
