using Application.Configuration.EnvironmentVariables;

namespace Persistence.Repositories.DatabaseCache;

public static class CacheConfiguration
{
    public static BoolEnvironmentVariable EnableCaching = new("ENABLE_CACHING", false, true);
}