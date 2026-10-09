using booking_service.Data;
using booking_service.Models;
using booking_service.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace booking_service.Controllers;

[ApiController]
[Route("api/bookings")]
public class BookingsController(BookingDbContext db, EventServiceClient events) : ControllerBase
{
    private static BookingResponse ToResponse(Booking b) =>
        new(b.Id, b.EventId, b.UserId, b.Seats, b.CreatedAt, b.UpdatedAt);

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private async Task EnsureCapacityRowAsync(Guid eventId, int capacity, CancellationToken ct)
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

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Reserve(CreateBookingRequest request, CancellationToken ct)
    {
        var eventId = request.EventId!.Value;
        var userId = request.UserId!;

        EventInfo? info;

        try
        {
            info = await events.GetEventAsync(eventId, ct);
        }
        catch (EventServiceUnavailableException)
        {
            return Problem("event-service is unreachable.", statusCode: StatusCodes.Status502BadGateway);
        }

        if (info is null) return NotFound();

        if (info.StartsAt <= DateTimeOffset.UtcNow)
            return Conflict(new ProblemDetails { Status = 409, Title = "Event has already started." });

        if (await db.Bookings.AnyAsync(b => b.EventId == eventId && b.UserId == userId, ct))
            return Conflict(new ProblemDetails { Status = 409, Title = "Booking already exists." });

        await EnsureCapacityRowAsync(eventId, info.Capacity, ct);

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var seats = request.Seats;
        
        var updated = await db.EventCapacities
            .Where(c => c.EventId == eventId && c.Reserved + seats <= c.Capacity)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Reserved, c => c.Reserved + seats), ct);
        
        if (updated == 0)
            return Conflict(new ProblemDetails { Status = 409, Title = "Event is full." });

        var now = DateTimeOffset.UtcNow;

        var booking = new Booking
        {
            EventId = eventId,
            UserId = userId,
            Seats = request.Seats,
            CreatedAt = now,
            UpdatedAt = now
        };

        db.Bookings.Add(booking);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            return Conflict(new ProblemDetails { Status = 409, Title = "Booking already exists." });
        }

        await tx.CommitAsync(ct);

        return CreatedAtAction(nameof(Get), new { id = booking.Id }, ToResponse(booking));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> Get(Guid id, CancellationToken ct)
    {
        var booking = await db.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct);
        return booking is null ? NotFound() : ToResponse(booking);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> Update(Guid id, UpdateBookingRequest request, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var booking = await db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (booking is null) return NotFound();

        var newSeats = request.Seats!.Value;
        var deltaSeats = newSeats - booking.Seats;

        if (deltaSeats != 0)
        {
            var eventId = booking.EventId;
            var updated = await db.EventCapacities
                .Where(c => c.EventId == eventId && c.Reserved + deltaSeats <= c.Capacity && c.Reserved + deltaSeats >= 0)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Reserved, c => c.Reserved + deltaSeats), ct);
            
            if (updated == 0)
                return Conflict(new ProblemDetails { Status = 409, Title = "Not enough remaining capacity." });

            booking.Seats = newSeats;
            booking.UpdatedAt = DateTimeOffset.UtcNow;
            
            await db.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);

        return ToResponse(booking);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Unreserve(Guid id, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var booking = await db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);
        if (booking is null) return NotFound();

        var eventId = booking.EventId;
        var seats = booking.Seats;
        
        await db.EventCapacities
            .Where(c => c.EventId == eventId)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.Reserved, c => Math.Max(c.Reserved - seats, 0)), ct);
        
        db.Bookings.Remove(booking);
        
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return NoContent();
    }

    [HttpGet("{eventId:guid}/availability")]
    public async Task<ActionResult<AvailabilityResponse>> Availability(Guid eventId, CancellationToken ct)
    {
        var cap = await db.EventCapacities.AsNoTracking().FirstOrDefaultAsync(c => c.EventId == eventId, ct);
        if (cap is not null)
            return new AvailabilityResponse(eventId, cap.Capacity, cap.Reserved, cap.Capacity - cap.Reserved);

        try
        {
            var info = await events.GetEventAsync(eventId, ct);
            if (info is null) return NotFound();
            return new AvailabilityResponse(eventId, info.Capacity, 0, info.Capacity);
        }
        catch (EventServiceUnavailableException)
        {
            return Problem("event-service is unreachable.", statusCode: StatusCodes.Status502BadGateway);
        }
    }
}
