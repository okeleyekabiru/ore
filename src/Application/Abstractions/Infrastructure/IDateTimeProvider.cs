using System;

namespace Ore.Application.Abstractions.Infrastructure;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
