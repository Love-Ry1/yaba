using event_service.Services;
using Microsoft.AspNetCore.Mvc;

namespace event_service.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
	protected ActionResult MapError<T>(ServiceResult<T> result) => result.Error switch
	{
		ServiceError.NotFound => NotFound(),
		ServiceError.Validation => ValidationError(result),
		_ => StatusCode(StatusCodes.Status500InternalServerError)
	};

	private ActionResult ValidationError<T>(ServiceResult<T> result)
	{
		ModelState.AddModelError(result.Field ?? string.Empty, result.Message ?? "Invalid value.");
		return ValidationProblem(ModelState);
	}
}
