using Reservations.Domain.Common;
using Reservations.Domain.Enums;
using Reservations.Domain.Pricing;

namespace Reservations.Domain.Entities;

/// <summary>
/// Aggregate root que representa una reserva de traslado.
/// Encapsula sus invariantes de ciclo de vida: las transiciones de estado solo pueden
/// ocurrir a través de <see cref="Confirm"/> y <see cref="Cancel"/>, que devuelven un
/// <see cref="Result"/> en lugar de lanzar excepciones para transiciones inválidas.
/// </summary>
public sealed class Reservation
{
    // Constructor privado: la única vía de creación es el factory Create.
    private Reservation(
        Guid id,
        string customerName,
        string origin,
        string destination,
        DateTime date,
        int passengers,
        ServiceType serviceType,
        decimal price,
        IReadOnlyList<PriceLine> priceBreakdown,
        DateTime createdAt)
    {
        Id = id;
        CustomerName = customerName;
        Origin = origin;
        Destination = destination;
        Date = date;
        Passengers = passengers;
        ServiceType = serviceType;
        Price = price;
        PriceBreakdown = priceBreakdown;
        Status = ReservationStatus.Created;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public Guid Id { get; }
    public string CustomerName { get; }
    public string Origin { get; }
    public string Destination { get; }
    public DateTime Date { get; }
    public int Passengers { get; }
    public ServiceType ServiceType { get; }
    public decimal Price { get; }
    public IReadOnlyList<PriceLine> PriceBreakdown { get; }
    public ReservationStatus Status { get; private set; }
    public DateTime CreatedAt { get; }
    public DateTime UpdatedAt { get; private set; }

    /// <summary>
    /// Crea una reserva en estado <see cref="ReservationStatus.Created"/> con su precio ya calculado.
    /// La validación del input (campos, rangos, fecha, duplicidad) ocurre en la capa de aplicación
    /// antes de llegar aquí; este factory garantiza la construcción consistente del agregado.
    /// </summary>
    public static Reservation Create(
        string customerName,
        string origin,
        string destination,
        DateTime date,
        int passengers,
        ServiceType serviceType,
        PriceQuote quote,
        DateTime now)
    {
        return new Reservation(
            Guid.NewGuid(),
            customerName.Trim(),
            origin.Trim(),
            destination.Trim(),
            date,
            passengers,
            serviceType,
            quote.Total,
            quote.Lines,
            now);
    }

    /// <summary>Confirma la reserva. Solo válido desde el estado Created.</summary>
    public Result Confirm(DateTime now)
    {
        if (Status == ReservationStatus.Confirmed)
            return Result.Failure(Error.Conflict("reservation.already_confirmed", "La reserva ya está confirmada."));

        if (Status == ReservationStatus.Cancelled)
            return Result.Failure(Error.Conflict("reservation.cancelled", "No se puede confirmar una reserva cancelada."));

        Status = ReservationStatus.Confirmed;
        UpdatedAt = now;
        return Result.Success();
    }

    /// <summary>Cancela la reserva. Válido desde Created o Confirmed.</summary>
    public Result Cancel(DateTime now)
    {
        if (Status == ReservationStatus.Cancelled)
            return Result.Failure(Error.Conflict("reservation.already_cancelled", "La reserva ya está cancelada."));

        Status = ReservationStatus.Cancelled;
        UpdatedAt = now;
        return Result.Success();
    }
}
