namespace booking_service.Services;

public enum ServiceError
{
    None,
    NotFound,
    Conflict,
    EventServiceUnavailable
}

public class ServiceResult<T>
{
    public T? Value { get; private init; }
    public ServiceError Error { get; private init; }
    public string? Message { get; private init; }
    public bool IsSuccess => Error == ServiceError.None;

    public static ServiceResult<T> Ok(T value) => new() { Value = value };
    public static ServiceResult<T> NotFound() => new() { Error = ServiceError.NotFound };
    public static ServiceResult<T> Conflict(string message) => new() { Error = ServiceError.Conflict, Message = message };
    public static ServiceResult<T> Unavailable() => new() { Error = ServiceError.EventServiceUnavailable };
}
