using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Reservations.Application.Reservations;
using Reservations.Application.Reservations.Abstractions;
using Reservations.Domain.Pricing;
using Reservations.Domain.Pricing.Rules;

namespace Reservations.Application;

/// <summary>Registro de servicios de la capa de Aplicación y Dominio (casos de uso, pricing, validadores).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IReservationService, ReservationService>();

        // Reglas de tarifación (patrón Strategy). El orden solo afecta el desglose, no el total,
        // porque todos los porcentajes se aplican sobre el mismo subtotal base.
        services.AddSingleton<ISurchargeRule, SameDaySurchargeRule>();
        services.AddSingleton<ISurchargeRule, LargeGroupSurchargeRule>();
        services.AddSingleton<ISurchargeRule, PremiumLargeGroupSurchargeRule>();
        services.AddSingleton<ISurchargeRule, AdvanceBookingDiscountRule>();
        services.AddSingleton<IReservationPricingEngine, ReservationPricingEngine>();

        // Validadores de FluentValidation descubiertos en este ensamblado.
        services.AddValidatorsFromAssemblyContaining<ReservationService>();

        return services;
    }
}
