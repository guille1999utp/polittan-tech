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
    public required string ServiceType { get; init; }
    public required string Status { get; init; }
    public required decimal Price { get; init; }
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
