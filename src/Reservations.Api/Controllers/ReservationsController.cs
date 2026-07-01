using Microsoft.AspNetCore.Mvc;
using Reservations.Api.Extensions;
using Reservations.Application.Reservations.Abstractions;
using Reservations.Application.Reservations.Dtos;

namespace Reservations.Api.Controllers;

/// <summary>
/// REST API for managing transfer reservations.
/// The controller is intentionally thin: it delegates all logic to the use case
/// (<see cref="IReservationService"/>) and only translates the result into HTTP.
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

    /// <summary>Creates a new reservation (initial status: Created) and computes its price.</summary>
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

    /// <summary>Gets all reservations (ordered from newest to oldest).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ReservationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ReservationResponse>>> GetAll(CancellationToken ct)
    {
        var reservations = await _service.GetAllAsync(ct);
        return Ok(reservations);
    }

    /// <summary>Gets a reservation by its identifier.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservationResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await _service.GetByIdAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error!.ToProblem();
    }

    /// <summary>Confirms a reservation (Created -> Confirmed).</summary>
    [HttpPatch("{id:guid}/confirm")]
    [ProducesResponseType(typeof(ReservationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ReservationResponse>> Confirm(Guid id, CancellationToken ct)
    {
        var result = await _service.ConfirmAsync(id, ct);
        return result.IsSuccess ? Ok(result.Value) : result.Error!.ToProblem();
    }

    /// <summary>Cancels a reservation (Created/Confirmed -> Cancelled).</summary>
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
