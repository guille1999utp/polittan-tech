using Reservations.Domain.Entities;

namespace Reservations.Application.Reservations.Dtos;

/// <summary>Output representation of a reservation (includes the price breakdown).</summary>
public sealed record ReservationResponse
{
    public required Guid Id { get; init; }
    public required string CustomerName { get; init; }
    public required string Origin { get; init; }
    public required string Destination { get; init; }
    public required DateTime Date { get; init; }
    public required int Passengers { get; init; }

    /// <summary>Service type. Possible values: <b>Standard</b>, <b>Premium</b>.</summary>
    /// <example>Premium</example>
    public required string ServiceType { get; init; }

    /// <summary>Reservation status. Possible values: <b>Created</b>, <b>Confirmed</b>, <b>Cancelled</b>.</summary>
    /// <example>Created</example>
    public required string Status { get; init; }

    /// <summary>Total price in COP (already rounded to whole pesos).</summary>
    /// <example>156000</example>
    public required decimal Price { get; init; }

    /// <summary>Detailed breakdown of how the total price was computed (base fare, surcharges, discounts).</summary>
    public required IReadOnlyList<PriceLineResponse> PriceBreakdown { get; init; }

    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static ReservationResponse FromEntity(Reservation r) => new()
    {
        Id = r.Id,
        CustomerName = r.CustomerName,
        Origin = r.Origin,
        Destination = r.Destination,
        Date = r.Date,
        Passengers = r.Passengers,
        ServiceType = r.ServiceType.ToString(),
        Status = r.Status.ToString(),
        Price = r.Price,
        PriceBreakdown = r.PriceBreakdown
            .Select(l => new PriceLineResponse(l.Concept, l.Amount))
            .ToList(),
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };
}

/// <summary>Price breakdown line in the response.</summary>
public sealed record PriceLineResponse(string Concept, decimal Amount);
