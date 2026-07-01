using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Reservations.Application.Reservations;
using Reservations.Application.Reservations.Abstractions;
using Reservations.Domain.Pricing;
using Reservations.Domain.Pricing.Rules;

namespace Reservations.Application;

/// <summary>Service registration for the Application and Domain layers (use cases, pricing, validators).</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IReservationService, ReservationService>();

        // Pricing rules (Strategy pattern). The order only affects the breakdown, not the total,
        // because all percentages are applied over the same base subtotal.
        services.AddSingleton<ISurchargeRule, SameDaySurchargeRule>();
        services.AddSingleton<ISurchargeRule, LargeGroupSurchargeRule>();
        services.AddSingleton<ISurchargeRule, PremiumLargeGroupSurchargeRule>();
        services.AddSingleton<ISurchargeRule, AdvanceBookingDiscountRule>();
        services.AddSingleton<IReservationPricingEngine, ReservationPricingEngine>();

        // FluentValidation validators discovered in this assembly.
        services.AddValidatorsFromAssemblyContaining<ReservationService>();

        return services;
    }
}
