using Application.Features.UserFeatures.UpdateUser;
using Common.ExpectedValues;

namespace UnitTests.Core.Applications.Features.UserFeatures.UpdateUser;

public class UpdateUserValidatorTests
{
    [Test, TestCaseSource(nameof(_validationTestCases))]
    public void ValidationTest(UpdateUserRequest request, ExpectedValidationResult expected)
    {
        var validator = new UpdateUserValidator();
        var results = validator.Validate(request);
        var resultErrors = results.ToDictionary();

        Assert.Multiple(() =>
        {
            Assert.That(results.IsValid, Is.EqualTo(expected.Result));
            Assert.That(resultErrors, Is.EqualTo(expected.Errors));
        });
    }

    private static object[] _validationTestCases =
    [
        new object []
        {
            new UpdateUserRequest
            {
               FirstName = "",
               LastName = "",
               Email = "",
            },
            new ExpectedValidationResult
            {
                Result = false,
                Errors = new Dictionary<string, string[]>
                {
                    {"FirstName", ["'First Name' must not be empty."]},
                    {"LastName", ["'Last Name' must not be empty."]},
                    {"Email", ["'Email' must not be empty.", "'Email' is not a valid email address."]},
                }
            }
        },
        new object []
        {
            new UpdateUserRequest
            {
               FirstName = null,
               LastName = null,
               Email = null,
            },
            new ExpectedValidationResult
            {
                Result = false,
                Errors = new Dictionary<string, string[]>
                {
                    {"FirstName", ["'First Name' must not be empty."]},
                    {"LastName", ["'Last Name' must not be empty."]},
                    {"Email", ["'Email' must not be empty."]},
                }
            }
        },
        new object []
        {
            new UpdateUserRequest
            {
               FirstName = "Albert",
               LastName = "Einstein",
               Email = "invalid email",
            },
            new ExpectedValidationResult
            {
                Result = false,
                Errors = new Dictionary<string, string[]>
                {
                   {"Email", ["'Email' is not a valid email address."]},
                }
            }
        },
        new object []
        {
            new UpdateUserRequest
            {
               FirstName = "Albert",
               LastName = "Einstein",
               Email = "albert.einstein@records.invalid",
            },
            new ExpectedValidationResult
            {
                Result = true,
                Errors = new Dictionary<string, string[]>()
            }
        },
    ];
}