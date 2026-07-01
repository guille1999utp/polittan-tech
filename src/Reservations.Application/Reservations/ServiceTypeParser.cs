using Reservations.Domain.Enums;

namespace Reservations.Application.Reservations;

/// <summary>Lenient (case-insensitive) parsing of the service type received as text.</summary>
public static class ServiceTypeParser
{
    public static bool TryParse(string? value, out ServiceType serviceType)
        => Enum.TryParse(value, ignoreCase: true, out serviceType) && Enum.IsDefined(serviceType);
}
