using booking_service.Data;
using booking_service.Models;

namespace booking_service.Services;

public interface IBookingService
{
    Task<ServiceResult<Booking>> ReserveAsync(Guid eventId, string userId, int seats, CancellationToken ct);
    Task<List<Booking>> ListForUserAsync(string userId, CancellationToken ct);
    Task<ServiceResult<Booking>> GetAsync(Guid id, CancellationToken ct);
    Task<ServiceResult<Booking>> UpdateAsync(Guid id, int newSeats, CancellationToken ct);
    Task<ServiceResult<bool>> UnreserveAsync(Guid id, CancellationToken ct);
    Task<ServiceResult<AvailabilityResponse>> GetAvailabilityAsync(Guid eventId, CancellationToken ct);
}
