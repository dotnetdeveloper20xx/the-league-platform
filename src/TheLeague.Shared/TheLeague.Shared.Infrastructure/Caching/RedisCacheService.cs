using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using StackExchange.Redis;
using TheLeague.Shared.Contracts.Services;

namespace TheLeague.Shared.Infrastructure.Caching;

public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IMemoryCache _memoryCache;
    private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(5);

    public RedisCacheService(IConnectionMultiplexer? redis, IMemoryCache memoryCache)
    {
        _redis = redis;
        _memoryCache = memoryCache;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (_redis is not null && _redis.IsConnected)
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(key);
            if (value.HasValue)
                return JsonSerializer.Deserialize<T>((string)value!, (JsonSerializerOptions?)null);
        }

        // Fallback to memory cache
        _memoryCache.TryGetValue(key, out T? cached);
        return cached;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null, CancellationToken ct = default)
    {
        var expiry = ttl ?? _defaultTtl;
        var serialized = JsonSerializer.Serialize(value);

        if (_redis is not null && _redis.IsConnected)
        {
            var db = _redis.GetDatabase();
            await db.StringSetAsync(key, serialized, expiry);
        }

        // Always set in memory cache as fallback
        _memoryCache.Set(key, value, expiry);
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        if (_redis is not null && _redis.IsConnected)
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
        }

        _memoryCache.Remove(key);
    }
}
