using ApolloSpoilers.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace ApolloSpoilers.Api.Common;

/// <summary>
/// Convenience helpers for mapping <see cref="Result{T}"/> into ActionResult.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<T> ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new { result.Error, result.ErrorCode }),
            "CONFLICT" => Conflict(new { result.Error, result.ErrorCode }),
            "VALIDATION" => BadRequest(new { result.Error, result.ErrorCode }),
            "UNAUTHORIZED" => Unauthorized(new { result.Error, result.ErrorCode }),
            "FORBIDDEN" => StatusCode(403, new { result.Error, result.ErrorCode }),
            _ => BadRequest(new { result.Error, result.ErrorCode })
        };
    }

    protected ActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return result.ErrorCode switch
        {
            "NOT_FOUND" => NotFound(new { result.Error, result.ErrorCode }),
            "CONFLICT" => Conflict(new { result.Error, result.ErrorCode }),
            "VALIDATION" => BadRequest(new { result.Error, result.ErrorCode }),
            "UNAUTHORIZED" => Unauthorized(new { result.Error, result.ErrorCode }),
            "FORBIDDEN" => StatusCode(403, new { result.Error, result.ErrorCode }),
            _ => BadRequest(new { result.Error, result.ErrorCode })
        };
    }
}
