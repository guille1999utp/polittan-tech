namespace Reservations.Domain.Common;

/// <summary>
/// Clasificación del error para poder mapearlo al código HTTP adecuado en la capa de API,
/// sin que el dominio conozca detalles de transporte.
/// </summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict
}

/// <summary>
/// Representa un error de negocio de forma explícita (código estable + mensaje legible).
/// Se usa junto con <see cref="Result"/> para evitar excepciones como control de flujo.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);
}
