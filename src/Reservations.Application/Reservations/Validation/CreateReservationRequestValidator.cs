using FluentValidation;
using Reservations.Application.Reservations.Dtos;

namespace Reservations.Application.Reservations.Validation;

/// <summary>
/// Validation of the creation payload. Covers required fields, the passenger range,
/// the date (valid and not in the past), distinct origin/destination and the supported service type.
/// </summary>
public sealed class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationRequestValidator(TimeProvider timeProvider)
    {
        var now = timeProvider.GetLocalNow().DateTime;

        // Stops each property's chain at its first failure (e.g. if the date is missing,
        // the rule that accesses its value is not evaluated). The rest of the fields are still
        // validated, so the 400 response lists all errors without breaking the request flow.
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Customer name is required.");

        RuleFor(x => x.Origin)
            .NotEmpty().WithMessage("Origin is required.");

        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("Destination is required.");

        RuleFor(x => x.Passengers)
            .NotNull().WithMessage("The number of passengers is required.")
            .InclusiveBetween(1, 6).WithMessage("The number of passengers must be between 1 and 6.");

        RuleFor(x => x.Date)
            .NotNull().WithMessage("Date is required.")
            .Must(date => date.HasValue && date.Value > now)
            .WithMessage("Date must be valid and not in the past.");

        RuleFor(x => x.ServiceType)
            .NotEmpty().WithMessage("Service type is required.")
            .Must(st => ServiceTypeParser.TryParse(st, out _))
            .WithMessage("Service type must be 'standard' or 'premium'.");

        // Origin and destination must be different (comparison ignoring case and surrounding spaces).
        RuleFor(x => x)
            .Must(x => !string.Equals(x.Origin?.Trim(), x.Destination?.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithName(nameof(CreateReservationRequest.Destination))
            .WithMessage("Origin and destination must be different.")
            .When(x => !string.IsNullOrWhiteSpace(x.Origin) && !string.IsNullOrWhiteSpace(x.Destination));
    }
}
