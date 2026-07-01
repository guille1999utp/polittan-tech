namespace Reservations.Application.Reservations.Dtos;

/// <summary>
/// Payload to create a reservation. Matches the JSON contract of the exercise.
/// <c>ServiceType</c> is received as text ("standard" | "premium") and validated/parsed in the application layer.
/// </summary>
public sealed record CreateReservationRequest
{
    public string? CustomerName { get; init; }
    public string? Origin { get; init; }
    public string? Destination { get; init; }
    public DateTime? Date { get; init; }
    public int? Passengers { get; init; }
    public string? ServiceType { get; init; }
}
