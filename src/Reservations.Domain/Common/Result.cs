namespace Reservations.Domain.Common;

/// <summary>
/// Resultado de una operación que puede fallar, sin lanzar excepciones.
/// Obliga a manejar el error de forma explícita en el llamador.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Un resultado exitoso no puede contener un error.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("Un resultado fallido debe contener un error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error? Error { get; }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, null);
    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

/// <summary>
/// Resultado que transporta un valor cuando la operación es exitosa.
/// </summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, Error? error) : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>Valor producido. Solo válido cuando <see cref="Result.IsSuccess"/> es verdadero.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No se puede acceder al valor de un resultado fallido.");

    public static implicit operator Result<T>(T value) => Success(value);
}
