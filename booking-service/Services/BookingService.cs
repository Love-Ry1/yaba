using booking_service.Data;
using booking_service.Models;
using booking_service.Repositories;

namespace booking_service.Services;

public class BookingService(IBookingRepository repo, EventServiceClient events) : IBookingService
{
    public async Task<ServiceResult<Booking>> ReserveAsync(Guid eventId, string userId, int seats, CancellationToken ct)
    {
        EventInfo? info;

        try
        {
            info = await events.GetEventAsync(eventId, ct);
        }
        catch (EventServiceUnavailableException)
        {
            return ServiceResult<Booking>.Unavailable();
        }

        if (info is null) return ServiceResult<Booking>.NotFound();

        if (info.StartsAt <= DateTimeOffset.UtcNow)
            return ServiceResult<Booking>.Conflict("Event has already started.");

        if (await repo.ExistsAsync(eventId, userId, ct))
            return ServiceResult<Booking>.Conflict("Booking already exists.");

        await repo.EnsureCapacityRowAsync(eventId, info.Capacity, ct);

        await using var tx = await repo.BeginTransactionAsync(ct);

        if (!await repo.TryAdjustReservedAsync(eventId, seats, ct))
            return ServiceResult<Booking>.Conflict("Event is full.");

        var now = DateTimeOffset.UtcNow;

        var booking = new Booking
        {
            EventId = eventId,
            UserId = userId,
            Seats = seats,
            CreatedAt = now,
            UpdatedAt = now
        };

        if (!await repo.TryAddAsync(booking, ct))
            return ServiceResult<Booking>.Conflict("Booking already exists.");

        await tx.CommitAsync(ct);

        return ServiceResult<Booking>.Ok(booking);
    }

    public Task<List<Booking>> ListForUserAsync(string userId, CancellationToken ct) =>
        repo.ListByUserAsync(userId, ct);

    public async Task<ServiceResult<Booking>> GetAsync(Guid id, CancellationToken ct)
    {
        var booking = await repo.GetByIdAsync(id, track: false, ct);

        return booking is null ? ServiceResult<Booking>.NotFound() : ServiceResult<Booking>.Ok(booking);
    }

    public async Task<ServiceResult<Booking>> UpdateAsync(Guid id, int newSeats, CancellationToken ct)
    {
        await using var tx = await repo.BeginTransactionAsync(ct);

        var booking = await repo.GetByIdAsync(id, track: true, ct);
        if (booking is null) return ServiceResult<Booking>.NotFound();

        var deltaSeats = newSeats - booking.Seats;

        if (deltaSeats != 0)
        {
            if (!await repo.TryAdjustReservedAsync(booking.EventId, deltaSeats, ct))
                return ServiceResult<Booking>.Conflict("Not enough remaining capacity.");

            booking.Seats = newSeats;
            booking.UpdatedAt = DateTimeOffset.UtcNow;

            await repo.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);

        return ServiceResult<Booking>.Ok(booking);
    }

    public async Task<ServiceResult<bool>> UnreserveAsync(Guid id, CancellationToken ct)
    {
        await using var tx = await repo.BeginTransactionAsync(ct);

        var booking = await repo.GetByIdAsync(id, track: true, ct);
        if (booking is null) return ServiceResult<bool>.NotFound();

        await repo.ReleaseSeatsAsync(booking.EventId, booking.Seats, ct);

        repo.Remove(booking);

        await repo.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<AvailabilityResponse>> GetAvailabilityAsync(Guid eventId, CancellationToken ct)
    {
        var cap = await repo.GetCapacityAsync(eventId, ct);
        if (cap is not null)
            return ServiceResult<AvailabilityResponse>.Ok(
                new AvailabilityResponse(eventId, cap.Capacity, cap.Reserved, cap.Capacity - cap.Reserved));

        try
        {
            var info = await events.GetEventAsync(eventId, ct);
            if (info is null) return ServiceResult<AvailabilityResponse>.NotFound();

            return ServiceResult<AvailabilityResponse>.Ok(
                new AvailabilityResponse(eventId, info.Capacity, 0, info.Capacity));
        }
        catch (EventServiceUnavailableException)
        {
            return ServiceResult<AvailabilityResponse>.Unavailable();
        }
    }
}
