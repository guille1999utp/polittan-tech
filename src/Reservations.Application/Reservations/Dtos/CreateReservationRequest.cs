namespace Reservations.Application.Reservations.Dtos;

/// <summary>
/// Payload to create a reservation. Matches the JSON contract of the exercise.
/// </summary>
public sealed record CreateReservationRequest
{
    /// <summary>
    /// Customer full name. Required, non-empty.
    /// </summary>
    /// <example>Juan Pérez</example>
    public string? CustomerName { get; init; }

    /// <summary>
    /// Pickup location. Required, non-empty. Must be different from 'destination'
    /// (compared ignoring case and surrounding spaces).
    /// </summary>
    /// <example>Bogotá</example>
    public string? Origin { get; init; }

    /// <summary>
    /// Drop-off location. Required, non-empty. Must be different from 'origin'.
    /// </summary>
    /// <example>Aeropuerto El Dorado</example>
    public string? Destination { get; init; }

    /// <summary>
    /// Reservation date/time in ISO 8601 format. Required and must be in the future (not in the past).
    /// </summary>
    /// <example>2026-12-20T10:00:00</example>
    public DateTime? Date { get; init; }

    /// <summary>
    /// Number of passengers. Required. Allowed range: 1 to 6 (inclusive).
    /// </summary>
    /// <example>3</example>
    public int? Passengers { get; init; }

    /// <summary>
    /// Service type. Required. Allowed values (case-insensitive): "standard" or "premium".
    /// standard = base fare 50,000 COP; premium = base fare 80,000 COP.
    /// </summary>
    /// <example>standard</example>
    public string? ServiceType { get; init; }
}
