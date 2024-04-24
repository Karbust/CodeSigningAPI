namespace Application.Common.Models;

public class CacheResult<TResult> where TResult : class?
{
    public TResult? Data { get; set; }
    public TimeSpan? CachedFor { get; set; }

    public DateTime? CachedUntil => CachedFor != null ? new DateTime?(DateTime.UtcNow.Add(CachedFor ?? TimeSpan.Zero)) : null;
}