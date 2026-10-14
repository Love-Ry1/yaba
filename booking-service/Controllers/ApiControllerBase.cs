using booking_service.Services;
using Microsoft.AspNetCore.Mvc;

namespace booking_service.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult MapError<T>(ServiceResult<T> result) => result.Error switch
    {
        ServiceError.NotFound => NotFound(),
        ServiceError.Conflict => Conflict(new ProblemDetails { Status = 409, Title = result.Message }),
        ServiceError.EventServiceUnavailable =>
            Problem("event-service is unreachable.", statusCode: StatusCodes.Status502BadGateway),
        _ => StatusCode(StatusCodes.Status500InternalServerError)
    };
}
