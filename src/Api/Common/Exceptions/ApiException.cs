using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Ore.Api.Common.Exceptions;

public class ApiException : Exception
{
    public ApiException(HttpStatusCode statusCode, string message, IEnumerable<string>? errors = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
        Errors = errors?.ToArray() ?? Array.Empty<string>();
    }

    public HttpStatusCode StatusCode { get; }

    public IReadOnlyCollection<string> Errors { get; }
}
