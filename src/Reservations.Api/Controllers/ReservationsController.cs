using Microsoft.AspNetCore.Mvc;
using Reservations.Api.Extensions;
using Reservations.Application.Reservations.Abstractions;
using Reservations.Application.Reservations.Dtos;

namespace Reservations.Api.Controllers;

/// <summary>
/// API REST para la gestión de reservas de traslados.
/// El controlador es intencionalmente delgado: delega toda la lógica al caso de uso
/// (<see cref="IReservationService"/>) y solo traduce el resultado a HTTP.
/// </summary>
[ApiController]
[Route("reservations")]
[Produces("application/json")]
public sealed class ReservationsController : ControllerBase
{
    private readonly IReservationService _service;

    public ReservationsController(IReservationService service)
    {
        _service = service;
    }

    /// <summary>Crea una nueva reserva (estado inicial: Created) y calcula su precio.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> Create(
        [FromBody] CreateReservationRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        if (result.IsFailure)
            return result.Error!.ToProblem();

        return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>Obtiene todas las reservas (ordenadas de la más reciente a la más antigua).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReservationResponse>>> GetAll(CancellationToken ct)
    {
        var reservations = await _service.GetAllAsync(ct);
        return Ok(reservations);
    }

    /// <summary>Obtiene una reserva por su identificador.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error!.ToProblem();
    }

    /// <summary>Confirma una reserva (Created -> Confirmed).</summary>
    [HttpPatch("{id:guid}/confirm")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> Confirm(Guid id, CancellationToken ct)
    {
        var result = await _service.ConfirmAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error!.ToProblem();
    }

    /// <summary>Cancela una reserva (Created/Confirmed -> Cancelled).</summary>
    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> Cancel(Guid id, CancellationToken ct)
    {
        var result = await _service.CancelAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error!.ToProblem();
    }
}
