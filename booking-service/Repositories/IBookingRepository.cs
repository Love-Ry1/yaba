using booking_service.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace booking_service.Repositories;

public interface IBookingRepository
{
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);
    Task<bool> ExistsAsync(Guid eventId, string userId, CancellationToken ct);
    Task<Booking?> GetByIdAsync(Guid id, bool track, CancellationToken ct);
    Task<List<Booking>> ListByUserAsync(string userId, CancellationToken ct);
    Task<EventCapacity?> GetCapacityAsync(Guid eventId, CancellationToken ct);
    Task EnsureCapacityRowAsync(Guid eventId, int capacity, CancellationToken ct);
    Task<bool> TryAdjustReservedAsync(Guid eventId, int delta, CancellationToken ct);
    Task ReleaseSeatsAsync(Guid eventId, int seats, CancellationToken ct);

    Task<bool> TryAddAsync(Booking booking, CancellationToken ct);
    void Remove(Booking booking);
    Task SaveChangesAsync(CancellationToken ct);
}
