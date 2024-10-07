using Application.Repositories.Database;
using Domain.Entities;

namespace Application.Repositories.DatabaseCache;

public class CachedUserRepository(
    ICacheRepository cacheRepository,
    IUserRepository userRepository,
    Serilog.ILogger logger
    ) : ICachedUserRepository
{
    private static string _cachePrefix = "user";
    private static int _defaultCacheExpirationSeconds = 60;

    public static string UserIdCacheKey(Guid id) => $"{_cachePrefix}::id:{id}";
    public static string UserEmailCacheKey(string email) => $"{_cachePrefix}::email:{email}";

    public async Task Create(User user, CancellationToken cancellationToken)
    {
        await userRepository.Create(user, cancellationToken);
        logger.Debug("created new user with {id}", user.Id);

        var cacheKey = UserIdCacheKey(user.Id);
        await cacheRepository.Set(cacheKey, user, _defaultCacheExpirationSeconds);
    }

    public async Task<User?> Get(Guid userId, CancellationToken cancellationToken)
    {
        var cacheKey = UserIdCacheKey(userId);
        var cachedUser = await cacheRepository.Get<User>(cacheKey);

        if (cachedUser.IsInCache)
            return cachedUser.Value;

        var user = await userRepository.Get(userId, cancellationToken);
        return user;
    }

    public async Task Update(User user, CancellationToken cancellationToken)
    {
        var cacheKey = UserIdCacheKey(user.Id);
        await userRepository.Update(user, cancellationToken);
        await cacheRepository.Set(cacheKey, user, _defaultCacheExpirationSeconds);
    }

    public Task Delete(User entity, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<User?> GetByEmail(string email, CancellationToken cancellationToken)
    {
        var user = userRepository.GetByEmail(email, cancellationToken);
        return user;
    }

    public async Task<bool> EmailExists(string email, CancellationToken cancellationToken)
    {
        var user = await GetByEmail(email, cancellationToken);
        return user != null;
    }
}