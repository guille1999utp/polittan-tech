using Microsoft.AspNetCore.Mvc;
using Reservations.Domain.Common;

namespace Reservations.Api.Extensions;

/// <summary>
/// Translates a domain <see cref="Error"/> into the proper HTTP response (ProblemDetails),
/// keeping the error-type -> HTTP-status mapping in a single place.
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
