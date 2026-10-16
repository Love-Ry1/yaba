using event_service.Data;
using Microsoft.EntityFrameworkCore;

namespace event_service.Repositories;

public class EventRepository(EventDbContext db) : IEventRepository
{
    public async Task AddAsync(Event entity, CancellationToken ct)
    {
        db.Events.Add(entity);
        await db.SaveChangesAsync(ct);
    }

    public Task<Event?> GetByIdAsync(Guid id, bool track, CancellationToken ct)
    {
        IQueryable<Event> query = track ? db.Events : db.Events.AsNoTracking();
        return query.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<(List<Event> Items, int Total)> ListAsync(
        DateTimeOffset? from, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Events.AsNoTracking().AsQueryable();
        if (from is { } f)
        {
            var utc = f.ToUniversalTime();
            query = query.Where(e => e.StartsAt >= utc);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(e => e.StartsAt).ThenBy(e => e.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
