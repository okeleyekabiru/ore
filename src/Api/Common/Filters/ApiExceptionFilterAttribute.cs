using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Ore.Api.Common.Exceptions;
using Ore.Api.Common.Responses;

namespace Ore.Api.Common.Filters;

public sealed class ApiExceptionFilterAttribute : ExceptionFilterAttribute
{
    private readonly ILogger<ApiExceptionFilterAttribute> _logger;

    public ApiExceptionFilterAttribute(ILogger<ApiExceptionFilterAttribute> logger)
    {
        _logger = logger;
    }

    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is ApiException apiException)
        {
            var errorResponse = BaseResponse.FailureResponse(apiException.Errors, apiException.Message);
            context.Result = new ObjectResult(errorResponse)
            {
                StatusCode = (int)apiException.StatusCode
            };
            context.ExceptionHandled = true;
            return;
        }

        _logger.LogError(context.Exception, "Unhandled exception encountered while processing request");

        var response = BaseResponse.FailureResponse(new[] { "An unexpected error occurred." });

        context.Result = new ObjectResult(response)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
        context.ExceptionHandled = true;
    }
}
