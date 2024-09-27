using FluentValidation;

namespace Application.Features.UserFeatures.CreateUser;

public class CreateUserValidator : AbstractValidator<CreateUserRequest>
{
    private const int PasswordMinLength = 8;

    public CreateUserValidator()
    {
        RuleFor(x => x.FirstName)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.LastName)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .EmailAddress();
        RuleFor(x => x.Password)
            .NotNull()
            .NotEmpty()
            .Must(chars => !chars.Any(c => Char.IsWhiteSpace(c)))
            .MinimumLength(PasswordMinLength);
    }
}