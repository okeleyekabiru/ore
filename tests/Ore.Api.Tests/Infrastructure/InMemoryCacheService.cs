using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Ore.Application.Abstractions.Infrastructure;

namespace Ore.Api.Tests.Infrastructure;

internal sealed class InMemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _store = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        if (_store.TryGetValue(key, out var entry) && !entry.IsExpired)
        {
            return Task.FromResult((T?)entry.Value);
        }

        _store.TryRemove(key, out _);
        return Task.FromResult(default(T));
    }

    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var expiresAt = ttl == TimeSpan.MaxValue ? (DateTime?)null : DateTime.UtcNow.Add(ttl);
        _store[key] = new CacheEntry(value, expiresAt);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    private sealed record CacheEntry(object? Value, DateTime? ExpiresAt)
    {
        public bool IsExpired => ExpiresAt is not null && DateTime.UtcNow > ExpiresAt.Value;
    }
}
