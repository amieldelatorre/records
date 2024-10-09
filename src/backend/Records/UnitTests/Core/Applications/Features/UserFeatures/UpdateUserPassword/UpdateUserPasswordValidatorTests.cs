using Application.Features.UserFeatures.UpdateUserPassword;
using Common.ExpectedValues;

namespace UnitTests.Core.Applications.Features.UserFeatures.UpdateUserPassword;

public class UpdateUserPasswordValidatorTests
{
    [Test, TestCaseSource(nameof(_validationTestCases))]
    public void ValidationTests(UpdateUserPasswordRequest request, ExpectedValidationResult expected)
    {
        var validator = new UpdateUserPasswordValidator();
        var results = validator.Validate(request);
        var resultsErrorDictionary = results.ToDictionary();

        Assert.Multiple(() =>
        {
            Assert.That(results.IsValid, Is.EqualTo(expected.Result));
            Assert.That(resultsErrorDictionary, Is.EqualTo(expected.Errors));
        });
    }

    private static object[] _validationTestCases =
    [
        new object[]
        {
            new UpdateUserPasswordRequest
            {
                CurrentPassword = "currentPassword",
                NewPassword = ""
            },
            new ExpectedValidationResult
            {
                Result = false,
                Errors = new Dictionary<string, string[]>
                {
                    {"NewPassword", ["'New Password' must not be empty.", "The length of 'New Password' must be at least 8 characters. You entered 0 characters."]},
                }
            }
        },
        new object[]
        {
            new UpdateUserPasswordRequest
            {
                CurrentPassword = "currentPassword",
                NewPassword = "pass"
            },
            new ExpectedValidationResult
            {
                Result = false,
                Errors = new Dictionary<string, string[]>
                {
                    {"NewPassword", ["The length of 'New Password' must be at least 8 characters. You entered 4 characters."]},
                }
            }
        },
        new object[]
        {
            new UpdateUserPasswordRequest
            {
                CurrentPassword = "currentPassword",
                NewPassword = "pass with space"
            },
            new ExpectedValidationResult
            {
                Result = false,
                Errors = new Dictionary<string, string[]>
                {
                    {"NewPassword", ["The 'Password' must not contain whitespace."]},
                }
            }
        },
        new object[]
        {
            new UpdateUserPasswordRequest
            {
                CurrentPassword = "currentPassword",
                NewPassword = null
            },
            new ExpectedValidationResult
            {
                Result = false,
                Errors = new Dictionary<string, string[]>
                {
                    {"NewPassword", ["'New Password' must not be empty."]},
                }
            }
        },
        new object[]
        {
            new UpdateUserPasswordRequest
            {
                CurrentPassword = "currentPassword",
                NewPassword = "password"
            },
            new ExpectedValidationResult
            {
                Result = true,
                Errors = new Dictionary<string, string[]>()
            }
        },
    ];
}