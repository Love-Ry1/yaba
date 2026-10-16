using event_service.Data;
using event_service.Models;

namespace event_service.Services;

public interface IEventService
{
    Task<ServiceResult<Event>> CreateAsync(CreateEventRequest request, CancellationToken ct);
    Task<ServiceResult<Event>> UpdateAsync(Guid id, UpdateEventRequest request, CancellationToken ct);
    Task<ServiceResult<Event>> GetAsync(Guid id, CancellationToken ct);
    Task<(List<Event> Items, int Page, int PageSize, int Total)> ListAsync(
        DateTimeOffset? from, int page, int pageSize, CancellationToken ct);
}
