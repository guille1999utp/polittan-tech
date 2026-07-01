using Microsoft.Extensions.DependencyInjection;
using Reservations.Application.Reservations.Abstractions;
using Reservations.Infrastructure.Persistence;

namespace Reservations.Infrastructure;

/// <summary>Service registration for the Infrastructure layer (persistence).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Singleton: the in-memory store must share state for the whole lifetime of the process.
        services.AddSingleton<IReservationRepository, InMemoryReservationRepository>();
        return services;
    }
}
