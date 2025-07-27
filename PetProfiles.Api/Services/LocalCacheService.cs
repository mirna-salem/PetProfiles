using System.Collections.Concurrent;

namespace PetProfiles.Api.Services;

public class LocalCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheItem> _cache;
    private readonly ILogger<InMemoryCacheService> _logger;
    private readonly Timer _cleanupTimer;

    public InMemoryCacheService(ILogger<InMemoryCacheService> logger)
    {
        _cache = new ConcurrentDictionary<string, CacheItem>();
        _logger = logger;
        
        // Clean up expired items every minute
        _cleanupTimer = new Timer(CleanupExpiredItems, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            if (_cache.TryGetValue(key, out var cacheItem))
            {
                if (cacheItem.IsExpired)
                {
                    _cache.TryRemove(key, out _);
                    _logger.LogDebug("Cache miss (expired) for key: {Key}", key);
                    return default;
                }

                _logger.LogDebug("Cache hit for key: {Key}", key);
                return (T)cacheItem.Value;
            }

            _logger.LogDebug("Cache miss for key: {Key}", key);
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache for key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        try
        {
            var expirationTime = expiration.HasValue 
                ? DateTime.UtcNow.Add(expiration.Value) 
                : DateTime.UtcNow.AddMinutes(10);

            var cacheItem = new CacheItem(value, expirationTime);
            _cache.AddOrUpdate(key, cacheItem, (k, v) => cacheItem);
            
            _logger.LogDebug("Cached value for key: {Key} with expiration: {Expiration}", key, expirationTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value in cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            if (_cache.TryRemove(key, out _))
            {
                _logger.LogDebug("Removed cache entry for key: {Key}", key);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entry for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var keysToRemove = _cache.Keys
                .Where(key => key.Contains(pattern.Replace("*", "")))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.TryRemove(key, out _);
            }

            _logger.LogDebug("Removed {Count} cache entries matching pattern: {Pattern}", keysToRemove.Count, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
        }
    }

    private void CleanupExpiredItems(object? state)
    {
        try
        {
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.IsExpired)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }

            if (expiredKeys.Count > 0)
            {
                _logger.LogDebug("Cleaned up {Count} expired cache entries", expiredKeys.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache cleanup");
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }

    private class CacheItem
    {
        public object Value { get; }
        public DateTime ExpirationTime { get; }
        public bool IsExpired => DateTime.UtcNow > ExpirationTime;

        public CacheItem(object value, DateTime expirationTime)
        {
            Value = value;
            ExpirationTime = expirationTime;
        }
    }
} 