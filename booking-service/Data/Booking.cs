namespace booking_service.Data;

public class Booking
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int Seats { get; set; } = 1;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public class EventCapacity
{
    public Guid EventId { get; set; }
    public int Capacity { get; set; }
    public int Reserved { get; set; }
}
