using System;
using Ore.Application.Abstractions.Infrastructure;

namespace Ore.Infrastructure.Services.Time;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
