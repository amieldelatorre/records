using Application.Features.UserFeatures.CreateUser;
using FluentValidation;

namespace Application.Features.UserFeatures.UpdateUserPassword;

public class UpdateUserPasswordValidator : AbstractValidator<UpdateUserPasswordRequest>
{
    public UpdateUserPasswordValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(SharedConfiguration.PasswordMinLength)
            .SetValidator(new PasswordWhiteSpaceValidator());
    }
}