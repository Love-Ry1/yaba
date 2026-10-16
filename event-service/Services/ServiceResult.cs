namespace event_service.Services;

public enum ServiceError
{
    None,
    NotFound,
    Validation
}

public class ServiceResult<T>
{
    public T? Value { get; private init; }
    public ServiceError Error { get; private init; }
    public string? Field { get; private init; }
    public string? Message { get; private init; }
    public bool IsSuccess => Error == ServiceError.None;

    public static ServiceResult<T> Ok(T value) => new() { Value = value };
    public static ServiceResult<T> NotFound() => new() { Error = ServiceError.NotFound };
    public static ServiceResult<T> Validation(string field, string message) =>
        new() { Error = ServiceError.Validation, Field = field, Message = message };
}
