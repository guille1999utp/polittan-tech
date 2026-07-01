namespace Reservations.Application.Reservations.Dtos;

/// <summary>
/// Payload para crear una reserva. Coincide con el contrato JSON del enunciado.
/// <c>ServiceType</c> se recibe como texto ("standard" | "premium") y se valida/parses en la capa de aplicación.
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
