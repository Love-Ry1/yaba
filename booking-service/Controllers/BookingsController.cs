using booking_service.Data;
using booking_service.Models;
using booking_service.Services;
using Microsoft.AspNetCore.Mvc;

namespace booking_service.Controllers;

[Route("api/bookings")]
public class BookingsController(IBookingService service) : ApiControllerBase
{
    private static BookingResponse ToResponse(Booking b) =>
        new(b.Id, b.EventId, b.UserId, b.Seats, b.CreatedAt, b.UpdatedAt);

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Reserve(CreateBookingRequest request, CancellationToken ct)
    {
        var result = await service.ReserveAsync(request.EventId!.Value, request.UserId!, request.Seats, ct);

        if (!result.IsSuccess) return MapError(result);

        var booking = result.Value!;
        return CreatedAtAction(nameof(Get), new { id = booking.Id }, ToResponse(booking));
    }

    [HttpGet]
    public async Task<ActionResult<List<BookingResponse>>> ListForUser([FromQuery] string userId, CancellationToken ct)
    {
        var items = await service.ListForUserAsync(userId, ct);

        return items.Select(ToResponse).ToList();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> Get(Guid id, CancellationToken ct)
    {
        var result = await service.GetAsync(id, ct);

        return result.IsSuccess ? ToResponse(result.Value!) : MapError(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> Update(Guid id, UpdateBookingRequest request, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request.Seats!.Value, ct);

        return result.IsSuccess ? ToResponse(result.Value!) : MapError(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Unreserve(Guid id, CancellationToken ct)
    {
        var result = await service.UnreserveAsync(id, ct);

        return result.IsSuccess ? NoContent() : MapError(result);
    }

    [HttpGet("{eventId:guid}/availability")]
    public async Task<ActionResult<AvailabilityResponse>> Availability(Guid eventId, CancellationToken ct)
    {
        var result = await service.GetAvailabilityAsync(eventId, ct);

        return result.IsSuccess ? result.Value! : MapError(result);
    }
}
