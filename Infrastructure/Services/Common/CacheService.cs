using Application.Common.Interfaces;
using EasyCaching.Core;
using Application.Common.Models;
using Infrastructure.Utils;

namespace Infrastructure.Services.Common;

public class CacheService(IEasyCachingProvider easyCachingProvider) : ICacheService
{
    public string GenerateKey(string prefix, params string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var key = prefix + "_" + string.Join('_', args);

        return key;
    }

    public async Task<TResult> GetAsync<TResult>(string cacheKey, Func<Task<TResult>> dataRetriever, TimeSpan expiration)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            return await dataRetriever.Invoke();
        }

        if (!await easyCachingProvider.ExistsAsync(cacheKey))
        {
            Console.WriteLine($"CacheService.GetAsync: Cache miss for key [{cacheKey}]");
            
            var cacheData = await dataRetriever.Invoke();

            var newCacheValue = SerializationHelper.Serialize(cacheData);

            await easyCachingProvider.SetAsync<string>(cacheKey, newCacheValue, expiration);

            return cacheData;
        }
        
        Console.WriteLine($"CacheService.GetAsync: Cache hit for key [{cacheKey}]");

        var cacheValue = await easyCachingProvider.GetAsync<string>(cacheKey);

        var result = SerializationHelper.Deserialize<TResult>(cacheValue.Value);

        return result;
    }

    public async Task<TResult> GetAsync<TResult>(string cacheKey, Func<TResult> dataRetriever, TimeSpan expiration)
    {
        if (string.IsNullOrWhiteSpace(cacheKey))
        {
            return dataRetriever.Invoke();
        }

        if (!await easyCachingProvider.ExistsAsync(cacheKey))
        {
            var cacheData = dataRetriever.Invoke();

            var newCacheValue = SerializationHelper.Serialize(cacheData);

            await easyCachingProvider.SetAsync<string>(cacheKey, newCacheValue, expiration);

            return cacheData;
        }

        var cacheValue = await easyCachingProvider.GetAsync<string>(cacheKey);

        var result = SerializationHelper.Deserialize<TResult>(cacheValue.Value);

        return result;
    }

    public async Task<CacheResult<TResult>> GetAsync<TResult>(string cacheKey) where TResult : class?
    {
        if (!await easyCachingProvider.ExistsAsync(cacheKey))
        {
            return default;
        }

        var cacheValue = await easyCachingProvider.GetAsync<string>(cacheKey);
        
        var expiration = await easyCachingProvider.GetExpirationAsync(cacheKey);

        var result = SerializationHelper.Deserialize<TResult>(cacheValue.Value);

        return new CacheResult<TResult>
        {
            Data = result,
            CachedFor = expiration,
        };
    }
    
    public async Task<T> GetTypedAsync<T>(string cacheKey)
    {
        if (!await easyCachingProvider.ExistsAsync(cacheKey))
        {
            return default;
        }
        
        var cacheValue = await easyCachingProvider.GetAsync<string>(cacheKey);
        
        var result = SerializationHelper.Deserialize<T>(cacheValue.Value);
        
        return result;
    }

    public Task SetAsync<TResult>(string cacheKey, TResult data, TimeSpan expiration)
    {
        return easyCachingProvider.SetAsync<string>(cacheKey, SerializationHelper.Serialize(data), expiration);
    }

    public Task RemoveAsync(string cacheKey)
    {
        return easyCachingProvider.RemoveAsync(cacheKey);
    }

    public Task FlushAsync()
    {
        return easyCachingProvider.FlushAsync();
    }
}