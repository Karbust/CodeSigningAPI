using Application.Common.Models;

namespace Application.Common.Interfaces;

public interface ICacheService
{
    string GenerateKey(string prefix, params string[] args);

    Task<TResult> GetAsync<TResult>(string cacheKey, Func<Task<TResult>> dataRetriever, TimeSpan expiration);
    
    Task<TResult> GetAsync<TResult>(string cacheKey, Func<TResult> dataRetriever, TimeSpan expiration);

    Task<CacheResult<TResult>> GetAsync<TResult>(string cacheKey) where TResult : class?;

    Task<T> GetTypedAsync<T>(string cacheKey);

    Task SetAsync<TResult>(string cacheKey, TResult data, TimeSpan expiration);

    Task FlushAsync();

    Task RemoveAsync(string cacheKey);
}