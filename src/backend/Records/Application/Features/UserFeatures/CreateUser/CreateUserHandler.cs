using Application.Repositories;
using Application.Repositories.Database;
using Domain.Entities;

namespace Application.Features.UserFeatures.CreateUser;

public class CreateUserHandler(IUserRepository _userRepository)
{
    public async Task<CreateUserResponse> Handle(CreateUserRequest request)
    {
        var validator = new CreateUserValidator();
        var validationResult = await validator.ValidateAsync(request);
        // TODO: Return validation result
        var user = CreateUserMapper.Map(request);
        // TODO: Check if email is unique before proceeding
        await _userRepository.Create(user);
        return CreateUserMapper.Map(user);
    }
}