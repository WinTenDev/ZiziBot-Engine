namespace ZiziBot.Application.Common.Types;

public class CacheParam<T>
{
    public required string CacheKey { get; set; }
    public required Func<Task<T>> Action { get; set; }
    public bool DisableCache { get; set; }
    public bool EvictBefore { get; set; }
    public bool EvictAfter { get; set; }
    public string? ExpireAfter { get; set; }
    public string? StaleAfter { get; set; }
    public bool ThrowIfError { get; set; }
}

public class CacheV2Param<T>
{
    public required string CacheKey { get; set; }
    public required Func<Task<CacheReturn<T>>> Action { get; set; }
    public bool DisableCache { get; set; }
    public bool EvictBefore { get; set; }
    public bool EvictAfter { get; set; }
    public string? ExpireAfter { get; set; }
    public string? StaleAfter { get; set; }
    public bool ThrowIfError { get; set; }
}