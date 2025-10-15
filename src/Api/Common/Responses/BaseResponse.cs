using System;
using System.Collections.Generic;
using System.Linq;

namespace Ore.Api.Common.Responses;

public record BaseResponse(bool Success, string? Message, IReadOnlyCollection<string> Errors)
{
    public static BaseResponse SuccessResponse(string? message = null) => new(true, message, Array.Empty<string>());

    public static BaseResponse FailureResponse(IEnumerable<string>? errors, string? message = null)
        => new(false, message, errors?.ToArray() ?? Array.Empty<string>());
}

public record BaseResponse<T>(T? Data, bool Success, string? Message, IReadOnlyCollection<string> Errors)
    : BaseResponse(Success, Message, Errors)
{
    public static BaseResponse<T> SuccessResponse(T data, string? message = null)
        => new(data, true, message, Array.Empty<string>());

    public static new BaseResponse<T> FailureResponse(IEnumerable<string>? errors, string? message = null)
        => new(default, false, message, errors?.ToArray() ?? Array.Empty<string>());
}
