using event_service.Data;
using event_service.Models;
using event_service.Repositories;

namespace event_service.Services;

public class EventService(IEventRepository repo) : IEventService
{
    private const int MaxPageSize = 100;
    private const string StartsAtMessage = "StartsAt must be in the future.";

    public async Task<ServiceResult<Event>> CreateAsync(CreateEventRequest request, CancellationToken ct)
    {
        if (request.StartsAt!.Value <= DateTimeOffset.UtcNow)
            return ServiceResult<Event>.Validation(nameof(request.StartsAt), StartsAtMessage);

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
        await repo.AddAsync(entity, ct);

        return ServiceResult<Event>.Ok(entity);
    }

    public async Task<ServiceResult<Event>> UpdateAsync(Guid id, UpdateEventRequest request, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(id, true, ct);
        if (entity is null) return ServiceResult<Event>.NotFound();

        if (request.StartsAt is { } startsAt)
        {
            if (startsAt <= DateTimeOffset.UtcNow)
                return ServiceResult<Event>.Validation(nameof(request.StartsAt), StartsAtMessage);
            entity.StartsAt = startsAt.ToUniversalTime();
        }
        if (request.Title is not null) entity.Title = request.Title;
        if (request.Description is not null) entity.Description = request.Description;
        if (request.Location is not null) entity.Location = request.Location;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await repo.SaveChangesAsync(ct);
        return ServiceResult<Event>.Ok(entity);
    }

    public async Task<ServiceResult<Event>> GetAsync(Guid id, CancellationToken ct)
    {
        var entity = await repo.GetByIdAsync(id, false, ct);
        return entity is null ? ServiceResult<Event>.NotFound() : ServiceResult<Event>.Ok(entity);
    }

    public async Task<(List<Event> Items, int Page, int PageSize, int Total)> ListAsync(
        DateTimeOffset? from, int page, int pageSize, CancellationToken ct)
    {
        if (page < 1) page = 1;
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var (items, total) = await repo.ListAsync(from, page, pageSize, ct);
        return (items, page, pageSize, total);
    }
}
