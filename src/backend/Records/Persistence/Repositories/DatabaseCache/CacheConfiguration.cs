using Persistence.Extensions;

namespace Persistence.Repositories.DatabaseCache;

public static class CacheConfiguration
{
    public static EnvironmentVariable<bool> EnableCaching = new("ENABLE_CACHING", false, true);
}