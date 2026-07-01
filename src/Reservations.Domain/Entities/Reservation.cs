using Reservations.Domain.Common;
using Reservations.Domain.Enums;
using Reservations.Domain.Pricing;

namespace Reservations.Domain.Entities;

/// <summary>
/// Aggregate root that represents a transfer reservation.
/// Encapsulates its lifecycle invariants: state transitions can only occur through
/// <see cref="Confirm"/> and <see cref="Cancel"/>, which return a <see cref="Result"/>
/// instead of throwing exceptions for invalid transitions.
/// </summary>
public sealed class Reservation
{
    // Private constructor: the only way to create an instance is the Create factory.
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
    /// Creates a reservation in <see cref="ReservationStatus.Created"/> state with its price already computed.
    /// Input validation (fields, ranges, date, duplication) happens in the application layer
    /// before reaching here; this factory guarantees a consistent construction of the aggregate.
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

    /// <summary>Confirms the reservation. Only valid from the Created state.</summary>
    public Result Confirm(DateTime now)
    {
        if (Status == ReservationStatus.Confirmed)
            return Result.Failure(Error.Conflict("reservation.already_confirmed", "The reservation is already confirmed."));

        if (Status == ReservationStatus.Cancelled)
            return Result.Failure(Error.Conflict("reservation.cancelled", "A cancelled reservation cannot be confirmed."));

        Status = ReservationStatus.Confirmed;
        UpdatedAt = now;
        return Result.Success();
    }

    /// <summary>Cancels the reservation. Valid from Created or Confirmed.</summary>
    public Result Cancel(DateTime now)
    {
        if (Status == ReservationStatus.Cancelled)
            return Result.Failure(Error.Conflict("reservation.already_cancelled", "The reservation is already cancelled."));

        Status = ReservationStatus.Cancelled;
        UpdatedAt = now;
        return Result.Success();
    }
}
