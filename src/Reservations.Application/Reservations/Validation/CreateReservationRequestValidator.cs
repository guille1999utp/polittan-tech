using FluentValidation;
using Reservations.Application.Reservations.Dtos;

namespace Reservations.Application.Reservations.Validation;

/// <summary>
/// Validación del payload de creación. Cubre los campos obligatorios, el rango de pasajeros,
/// la fecha (válida y no en el pasado), origen/destino distintos y el tipo de servicio soportado.
/// </summary>
public sealed class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator(TimeProvider timeProvider)
    {
        var now = timeProvider.GetLocalNow().DateTime;

        // Detiene la cadena de cada propiedad en su primer fallo (p. ej. si falta la fecha,
        // no se evalúa la regla que accede a su valor). Se siguen validando el resto de campos,
        // de modo que la respuesta 400 lista todos los errores sin que la petición rompa el flujo.
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("El nombre del cliente es obligatorio.");

        RuleFor(x => x.Origin)
            .NotEmpty().WithMessage("El origen es obligatorio.");

        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("El destino es obligatorio.");

        RuleFor(x => x.Passengers)
            .NotNull().WithMessage("El número de pasajeros es obligatorio.")
            .InclusiveBetween(1, 6).WithMessage("El número de pasajeros debe estar entre 1 y 6.");

        RuleFor(x => x.Date)
            .NotNull().WithMessage("La fecha es obligatoria.")
            .Must(date => date.HasValue && date.Value > now)
            .WithMessage("La fecha debe ser válida y no estar en el pasado.");

        RuleFor(x => x.ServiceType)
            .NotEmpty().WithMessage("El tipo de servicio es obligatorio.")
            .Must(st => ServiceTypeParser.TryParse(st, out _))
            .WithMessage("El tipo de servicio debe ser 'standard' o 'premium'.");

        // Origen y destino deben ser distintos (comparación sin distinguir mayúsculas ni espacios).
        RuleFor(x => x)
            .Must(x => !string.Equals(x.Origin?.Trim(), x.Destination?.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithName(nameof(CreateReservationRequest.Destination))
            .WithMessage("El origen y el destino deben ser distintos.")
            .When(x => !string.IsNullOrWhiteSpace(x.Origin) && !string.IsNullOrWhiteSpace(x.Destination));
    }
}
