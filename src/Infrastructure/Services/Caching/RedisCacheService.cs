using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Ore.Application.Abstractions.Infrastructure;
using StackExchange.Redis;

namespace Ore.Infrastructure.Services.Caching;

public sealed class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var database = GetDatabase();
        var value = await database.StringGetAsync(key).ConfigureAwait(false);
        if (!value.HasValue)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken cancellationToken = default)
    {
        var database = GetDatabase();
        var payload = JsonSerializer.Serialize(value);
        await database.StringSetAsync(key, payload, ttl).ConfigureAwait(false);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var database = GetDatabase();
        await database.KeyDeleteAsync(key).ConfigureAwait(false);
    }

    private IDatabase GetDatabase() => _connectionMultiplexer.GetDatabase();
}
