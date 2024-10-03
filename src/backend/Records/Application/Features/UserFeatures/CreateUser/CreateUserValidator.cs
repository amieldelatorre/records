using FluentValidation;

namespace Application.Features.UserFeatures.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    private const int PasswordMinLength = 8;

    public CreateUserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty();
        RuleFor(x => x.LastName)
            .NotEmpty();
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(PasswordMinLength)
            .SetValidator(new PasswordWhiteSpaceValidator());
    }
}
