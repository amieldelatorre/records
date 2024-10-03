using FluentValidation;

namespace Application.Features.UserFeatures;
public class PasswordWhiteSpaceValidator : AbstractValidator<string>
{
    public PasswordWhiteSpaceValidator()
    {
        RuleFor(x => x)
            .Must(chars => !chars.Any(c => char.IsWhiteSpace(c)))
            .WithMessage("The 'Password' must not contain whitespace.");
    }
}
