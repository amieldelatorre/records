using Application.Repositories;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.UserFeatures.CreateUser;

public class CreateUserHandler(IUserRepository _userRepository, IMapper _mapper)
{
    public async Task<CreateUserResponse> Handle(CreateUserRequest request)
    {
        var validator = new CreateUserValidator();
        var validationResult = await validator.ValidateAsync(request);
        var user = _mapper.Map<User>(request);
        // TODO: Check if email is unique before proceeding
        await _userRepository.Create(user);
        return _mapper.Map<CreateUserResponse>(user);
    }
}