using booking_service.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace booking_service.Repositories;

public class BookingRepository(BookingDbContext db) : IBookingRepository
{
    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct) =>
        db.Database.BeginTransactionAsync(ct);

    public Task<bool> ExistsAsync(Guid eventId, string userId, CancellationToken ct) =>
        db.Bookings.AnyAsync(b => b.EventId == eventId && b.UserId == userId, ct);

    public Task<Booking?> GetByIdAsync(Guid id, bool track, CancellationToken ct)
    {
        IQueryable<Booking> query = track ? db.Bookings : db.Bookings.AsNoTracking();
        return query.FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public Task<List<Booking>> ListByUserAsync(string userId, CancellationToken ct) =>
        db.Bookings.AsNoTracking()
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);

    public Task<EventCapacity?> GetCapacityAsync(Guid eventId, CancellationToken ct) =>
        db.EventCapacities.AsNoTracking().FirstOrDefaultAsync(c => c.EventId == eventId, ct);

    public async Task EnsureCapacityRowAsync(Guid eventId, int capacity, CancellationToken ct)
    {
        if (await db.EventCapacities.AnyAsync(c => c.EventId == eventId, ct)) return;

        var row = new EventCapacity { EventId = eventId, Capacity = capacity, Reserved = 0 };
        db.EventCapacities.Add(row);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            db.Entry(row).State = EntityState.Detached;
        }
    }

    public async Task<bool> TryAdjustReservedAsync(Guid eventId, int delta, CancellationToken ct)
    {
        var updated = await db.EventCapacities
            .Where(c => c.EventId == eventId && c.Reserved + delta <= c.Capacity && c.Reserved + delta >= 0)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Reserved, c => c.Reserved + delta), ct);

        return updated > 0;
    }

    public Task ReleaseSeatsAsync(Guid eventId, int seats, CancellationToken ct) =>
        db.EventCapacities
            .Where(c => c.EventId == eventId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Reserved, c => Math.Max(c.Reserved - seats, 0)), ct);

    public async Task<bool> TryAddAsync(Booking booking, CancellationToken ct)
    {
        db.Bookings.Add(booking);

        try
        {
            await db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return false;
        }
    }

    public void Remove(Booking booking) => db.Bookings.Remove(booking);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
