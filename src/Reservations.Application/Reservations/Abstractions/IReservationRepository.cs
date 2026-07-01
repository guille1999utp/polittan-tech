using Reservations.Domain.Entities;

namespace Reservations.Application.Reservations.Abstractions;

/// <summary>
/// Abstracción de persistencia de reservas. La implementación concreta (in-memory, EF Core, etc.)
/// vive en la capa de Infraestructura; la capa de Aplicación depende solo de esta interfaz.
/// Es asíncrona para no atar el contrato a un almacenamiento síncrono.
/// </summary>
public interface IReservationRepository
{
    Task<Reservation> AddAsync(Reservation reservation, CancellationToken ct = default);
    Task<IReadOnlyList<Reservation>> GetAllAsync(CancellationToken ct = default);
    Task<Reservation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateAsync(Reservation reservation, CancellationToken ct = default);

    /// <summary>
    /// Verifica la regla de duplicidad: existe una reserva con la misma combinación de
    /// cliente, origen, destino, fecha y tipo de servicio (ignorando las canceladas).
    /// </summary>
    Task<bool> ExistsDuplicateAsync(
        string customerName,
        string origin,
        string destination,
        DateTime date,
        Domain.Enums.ServiceType serviceType,
        CancellationToken ct = default);
}
