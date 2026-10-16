using event_service.Data;

namespace event_service.Repositories;

public interface IEventRepository
{
    Task AddAsync(Event entity, CancellationToken ct);
    Task<Event?> GetByIdAsync(Guid id, bool track, CancellationToken ct);
    Task<(List<Event> Items, int Total)> ListAsync(DateTimeOffset? from, int page, int pageSize, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
