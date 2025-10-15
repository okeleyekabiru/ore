using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ore.Api.Common.Responses;
using Ore.Application.Common.Models;

namespace Ore.Api.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected ApiControllerBase(IMediator mediator)
    {
        Mediator = mediator;
    }

    protected IMediator Mediator { get; }

    protected IActionResult FromResult<T>(Result<T> result, string? message = null)
    {
        if (result.Succeeded)
        {
            if (result.Value is null)
            {
                return Ok(BaseResponse.SuccessResponse(message));
            }

            return Ok(BaseResponse<T>.SuccessResponse(result.Value, message));
        }

        return BadRequest(BaseResponse.FailureResponse(result.Errors, message));
    }
}
