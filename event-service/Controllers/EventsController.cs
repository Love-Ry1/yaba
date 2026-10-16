using event_service.Data;
using event_service.Models;
using event_service.Services;
using Microsoft.AspNetCore.Mvc;

namespace event_service.Controllers;

[Route("api/events")]
public class EventsController(IEventService service) : ApiControllerBase
{
    private static EventResponse ToResponse(Event e) =>
        new(e.Id, e.Title, e.Description, e.Location, e.StartsAt, e.Capacity, e.CreatedAt, e.UpdatedAt);

    [HttpPost]
    public async Task<ActionResult<EventResponse>> Create(CreateEventRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        if (!result.IsSuccess) return MapError(result);

        var entity = result.Value!;
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, ToResponse(entity));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<EventResponse>> Update(Guid id, UpdateEventRequest request, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result.IsSuccess ? ToResponse(result.Value!) : MapError(result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<EventResponse>>> List(
        [FromQuery] DateTimeOffset? from, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var (items, p, ps, total) = await service.ListAsync(from, page, pageSize, ct);
        return new PagedResponse<EventResponse>(items.Select(ToResponse).ToList(), p, ps, total);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventResponse>> Get(Guid id, CancellationToken ct)
    {
        var result = await service.GetAsync(id, ct);
        return result.IsSuccess ? ToResponse(result.Value!) : MapError(result);
    }
}
