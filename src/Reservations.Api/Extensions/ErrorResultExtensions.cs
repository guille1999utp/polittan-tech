using Microsoft.AspNetCore.Mvc;
using Reservations.Domain.Common;

namespace Reservations.Api.Extensions;

/// <summary>
/// Traduce un <see cref="Error"/> de dominio a la respuesta HTTP adecuada (ProblemDetails),
/// manteniendo el mapeo tipo-de-error -> código HTTP en un único lugar.
/// </summary>
public static class ErrorResultExtensions
{
    public static ActionResult ToProblem(this Error error)
    {
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var problem = new ProblemDetails
        {
            Status = status,
            Title = error.Message,
            Extensions = { ["code"] = error.Code }
        };

        return new ObjectResult(problem) { StatusCode = status };
    }
}
