using System.ComponentModel.DataAnnotations;

namespace event_service.Models;

public class CreateEventRequest
{
    [Required, StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [Required]
    public DateTimeOffset? StartsAt { get; set; }

    [Required, Range(1, int.MaxValue)]
    public int? Capacity { get; set; }
}

public class UpdateEventRequest
{
    [StringLength(200, MinimumLength = 1)]
    public string? Title { get; set; }

    public string? Description { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    public DateTimeOffset? StartsAt { get; set; }
}

public record EventResponse(
    Guid Id,
    string Title,
    string? Description,
    string? Location,
    DateTimeOffset StartsAt,
    int Capacity,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public record PagedResponse<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
