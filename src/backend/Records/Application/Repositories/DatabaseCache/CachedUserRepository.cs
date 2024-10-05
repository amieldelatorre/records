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

    public async Task<User> Create(User entity)
    {
        var newUser = await userRepository.Create(entity);
        logger.Debug("created new user with {id}", newUser.Id);

        var cacheKey = UserIdCacheKey(newUser.Id);
        await cacheRepository.Set(cacheKey, newUser, _defaultCacheExpirationSeconds);
        return newUser;
    }

    public async Task<User?> Get(Guid userId)
    {
        var cacheKey = UserIdCacheKey(userId);
        var cachedUser = await cacheRepository.Get<User>(cacheKey);

        if (cachedUser.IsInCache)
            return cachedUser.Value;

        var user = await userRepository.Get(userId);
        return user;
    }

    public Task<User> Update(User entity)
    {
        throw new NotImplementedException();
    }

    public Task Delete(User entity)
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