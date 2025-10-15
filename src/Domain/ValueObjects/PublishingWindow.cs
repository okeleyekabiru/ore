using System;
using Ore.Domain.Common;

namespace Ore.Domain.ValueObjects;

public sealed class PublishingWindow : ValueObject
{
    public DateTime PublishOnUtc { get; }
    public TimeSpan? RetryInterval { get; }
    public int MaxRetryCount { get; }

    private PublishingWindow(DateTime publishOnUtc, TimeSpan? retryInterval, int maxRetryCount)
    {
        PublishOnUtc = publishOnUtc;
        RetryInterval = retryInterval;
        MaxRetryCount = maxRetryCount;
    }

    public static PublishingWindow Create(DateTime publishOnUtc, TimeSpan? retryInterval = null, int maxRetryCount = 3)
    {
        if (publishOnUtc <= DateTime.UtcNow.AddMinutes(-1))
        {
            throw new ArgumentException("PublishOnUtc must be in the future", nameof(publishOnUtc));
        }

        if (maxRetryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxRetryCount));
        }

        return new PublishingWindow(publishOnUtc, retryInterval, maxRetryCount);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return PublishOnUtc;
        yield return RetryInterval;
        yield return MaxRetryCount;
    }
}
