using Application.Repositories;
using Application.Repositories.Database;
using Application.Repositories.DatabaseCache;
using Domain.Entities;

namespace Application.Features.UserFeatures.CreateUser;

public class CreateUserHandler(IUserDatabaseCacheRepository userRepository)
{
    public async Task<CreateUserResponse> Handle(CreateUserRequest request)
    {
        var validator = new CreateUserValidator();
        var validationResult = await validator.ValidateAsync(request);
        // TODO: Return validation result
        var user = CreateUserMapper.Map(request);
        // TODO: Check if email is unique before proceeding
        await userRepository.Create(user);
        return CreateUserMapper.Map(user);
    }
}