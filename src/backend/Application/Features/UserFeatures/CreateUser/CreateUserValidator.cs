using Application.Repositories.Database;
using FluentValidation;

namespace Application.Features.UserFeatures.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserValidator(IUserRepository userRepository)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MustAsync(async (email, cancellation)=>
            {
                var exists = await userRepository.EmailExists(email, cancellation);
                return !exists;
            }).WithMessage("'Email' provided is already in use.");
        RuleFor(x => x.Username)
            .NotEmpty()
            .MinimumLength(Application.Features.SharedConfiguration.DefaultUsernameMinimumLength)
            .Matches(Application.Features.SharedConfiguration.DefaultAllowedUsernamePattern)
                .WithMessage("'Username' can only contain lowercase letters and numbers.")
            .MustAsync(async (username, cancellation)=>
            {
                var exists = await userRepository.UsernameExists(username, cancellation);
                return !exists;
            }).WithMessage("'Username' provided is already in use.");
        
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(SharedConfiguration.PasswordMinLength)
            .SetValidator(new PasswordWhiteSpaceValidator());
    }
}