namespace Reservations.Domain.Common;

/// <summary>
/// Error classification, used to map it to the proper HTTP status code in the API layer
/// without the domain having to know about transport details.
/// </summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict
}

/// <summary>
/// Represents a business error explicitly (stable code + human-readable message).
/// Used together with <see cref="Result"/> to avoid exceptions as control flow.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
}
