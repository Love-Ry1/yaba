using event_service.Data;
using event_service.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace event_service.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(EventDbContext db) : ControllerBase
{
    private const int MaxPageSize = 100;

    private static EventResponse ToResponse(Event e) =>
        new(e.Id, e.Title, e.Description, e.Location, e.StartsAt, e.Capacity, e.CreatedAt, e.UpdatedAt);

    [HttpPost]
    public async Task<ActionResult<EventResponse>> Create(CreateEventRequest request, CancellationToken ct)
    {
        if (request.StartsAt!.Value <= DateTimeOffset.UtcNow)
        {
            ModelState.AddModelError(nameof(request.StartsAt), "StartsAt must be in the future.");
            return ValidationProblem(ModelState);
        }

        var now = DateTimeOffset.UtcNow;
        var entity = new Event
        {
            Title = request.Title!,
            Description = request.Description,
            Location = request.Location,
            StartsAt = request.StartsAt.Value.ToUniversalTime(),
            Capacity = request.Capacity!.Value,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Events.Add(entity);
        await db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = entity.Id }, ToResponse(entity));
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<EventResponse>> Update(Guid id, UpdateEventRequest request, CancellationToken ct)
    {
        var entity = await db.Events.FindAsync([id], ct);
        if (entity is null) return NotFound();

        if (request.StartsAt is { } startsAt)
        {
            if (startsAt <= DateTimeOffset.UtcNow)
            {
                ModelState.AddModelError(nameof(request.StartsAt), "StartsAt must be in the future.");
                return ValidationProblem(ModelState);
            }
            entity.StartsAt = startsAt.ToUniversalTime();
        }
        if (request.Title is not null) entity.Title = request.Title;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.Location is not null) entity.Location = request.Location;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return ToResponse(entity);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<EventResponse>>> List(
        [FromQuery] DateTimeOffset? from, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = db.Events.AsNoTracking().AsQueryable();
        if (from is { } f) query = query.Where(e => e.StartsAt >= f.ToUniversalTime());

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.StartsAt).ThenBy(e => e.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<EventResponse>(items.Select(ToResponse).ToList(), page, pageSize, total);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EventResponse>> Get(Guid id, CancellationToken ct)
    {
        var entity = await db.Events.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, ct);
        return entity is null ? NotFound() : ToResponse(entity);
    }
}
