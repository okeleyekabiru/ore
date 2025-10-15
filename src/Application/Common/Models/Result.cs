using System;

namespace Ore.Application.Common.Models;

public sealed class Result<T>
{
    private Result(bool succeeded, T? value, IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Value = value;
        Errors = errors;
    }

    public bool Succeeded { get; }
    public T? Value { get; }
    public IReadOnlyCollection<string> Errors { get; }

    public static Result<T> Success(T value) => new(true, value, Array.Empty<string>());

    public static Result<T> Failure(params string[] errors) => new(false, default, errors);
}
