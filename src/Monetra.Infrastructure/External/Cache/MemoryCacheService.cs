using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Monetra.Core.Interfaces;

namespace Monetra.Infrastructure.External.Cache;

/// <summary>
/// Implementação fallback do cache em memória (para desenvolvimento sem Redis).
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(ILogger<MemoryCacheService> logger)
    {
        _logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            if (entry.ExpiresAt > DateTime.UtcNow)
            {
                _logger.LogDebug("Memory Cache HIT: {Key}", key);
                return Task.FromResult(entry.Value as T);
            }
            else
            {
                // Remover expirado
                _cache.TryRemove(key, out _);
                _logger.LogDebug("Memory Cache EXPIRED: {Key}", key);
            }
        }

        _logger.LogDebug("Memory Cache MISS: {Key}", key);
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var entry = new CacheEntry
        {
            Value = value,
            ExpiresAt = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromMinutes(30))
        };

        _cache.AddOrUpdate(key, entry, (_, _) => entry);
        _logger.LogDebug("Memory Cache SET: {Key}", key);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryRemove(key, out _);
        _logger.LogDebug("Memory Cache REMOVE: {Key}", key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var entry))
        {
            return Task.FromResult(entry.ExpiresAt > DateTime.UtcNow);
        }

        return Task.FromResult(false);
    }

    public async Task<T?> GetOrCreateAsync<T>(
        string key,
        Func<Task<T?>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
            return cached;

        var value = await factory();
        if (value != null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
