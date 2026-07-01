using Reservations.Domain.Enums;

namespace Reservations.Application.Reservations;

/// <summary>Parseo tolerante (case-insensitive) del tipo de servicio recibido como texto.</summary>
public static class ServiceTypeParser
{
    public static bool TryParse(string? value, out ServiceType serviceType)
        => Enum.TryParse(value, ignoreCase: true, out serviceType) && Enum.IsDefined(serviceType);
}
