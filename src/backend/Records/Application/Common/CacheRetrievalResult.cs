namespace Application.Common;

public struct CacheRetrievalResult<T>
{
    public T? Value;
    public bool IsInCache { get; }

    public CacheRetrievalResult(bool isInCache, T value)
    {
        Value = value;
        IsInCache = isInCache;
    }

    public CacheRetrievalResult(bool isInCache)
    {
        IsInCache = isInCache;
    }
}