using Microsoft.Extensions.DependencyInjection;
using Reservations.Application.Reservations.Abstractions;
using Reservations.Infrastructure.Persistence;

namespace Reservations.Infrastructure;

/// <summary>Registro de servicios de la capa de Infraestructura (persistencia).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Singleton: el almacén in-memory debe compartir estado durante toda la vida del proceso.
        services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
        return services;
    }
}
