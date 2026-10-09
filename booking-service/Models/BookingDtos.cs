using System.ComponentModel.DataAnnotations;

namespace booking_service.Models;

public record CreateBookingRequest(
    [property: Required] Guid? EventId,
    [property: Required, StringLength(100, MinimumLength = 1)] string? UserId,
    [property: Range(1, 10)] int Seats = 1);

public record UpdateBookingRequest(
    [property: Required, Range(1, 10)] int? Seats);

public record BookingResponse(
    Guid Id,
    Guid EventId,
    string UserId,
    int Seats,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record AvailabilityResponse(Guid EventId, int Capacity, int Reserved, int Remaining);

public record EventInfo(Guid Id, DateTimeOffset StartsAt, int Capacity);
