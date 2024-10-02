using Application.Features.UserFeatures.CreateUser;
using Common.ExpectedValues;

namespace UnitTests.UserFeatures.CreateUser;

public class CreateUserValidatorTests
{
    internal static object[] ValidationTestsProvider =
    [
        new object[]
        {
           new CreateUserRequest()
           {
               FirstName = "",
               LastName = "",
               Email = "",
               Password = ""
           },
           new ExpectedValidationResult()
           {
               Result = false,
               Errors = new Dictionary<string, string[]>
               {
                   {"FirstName", ["'First Name' must not be empty."]},
                   {"LastName", ["'Last Name' must not be empty."]},
                   {"Email", ["'Email' must not be empty.", "'Email' is not a valid email address."]},
                   {"Password", ["'Password' must not be empty.", "The length of 'Password' must be at least 8 characters. You entered 0 characters."]},
               }
           }
        },
        new object[]
        {
            new CreateUserRequest()
            {
                FirstName = "Valid",
                LastName = "Valid",
                Email = "mail@example.invalid",
                Password = "notval"
            },
            new ExpectedValidationResult()
            {
                Result = false,
                Errors = new Dictionary<string, string[]>
                {
                    {"Password", ["The length of 'Password' must be at least 8 characters. You entered 6 characters."]},
                }
            }
        },
        new object[]
        {
            new CreateUserRequest()
            {
                FirstName = "Valid",
                LastName = "Valid",
                Email = "mail@example.invalid",
                Password = "not val"
            },
            new ExpectedValidationResult()
            {
                Result = false,
                Errors = new Dictionary<string, string[]>
                {
                    {"Password", ["The length of 'Password' must be at least 8 characters. You entered 7 characters.", "The 'Password' must not contain whitespace."]},
                }
            }
        },
        new object[]
        {
            new CreateUserRequest()
            {
                FirstName = null,
                LastName = null,
                Email = null,
                Password = null
            },
            new ExpectedValidationResult()
            {
                Result = false,
                Errors = new Dictionary<string, string[]>
                {
                   {"FirstName", ["'First Name' must not be empty."]},
                   {"LastName", ["'Last Name' must not be empty."]},
                   {"Email", ["'Email' must not be empty."]},
                   {"Password", ["'Password' must not be empty."]}
                }
            }
        },
        new object[]
        {
            new CreateUserRequest()
            {
                FirstName = "Albert",
                LastName = "Einstein",
                Email = "albert.einstein@example.invalid",
                Password = "hispassword"
            },
            new ExpectedValidationResult()
            {
                Result = true,
                Errors = new Dictionary<string, string[]>()
            }
        },
        new object[]
        {
            new CreateUserRequest()
            {
                FirstName = " Albert    ",
                LastName = "  Einstein ",
                Email = " albert.einstein@example.invalid         ",
                Password = "hispassword"
            },
            new ExpectedValidationResult()
            {
                Result = true,
                Errors = new Dictionary<string, string[]>()
            }
        },
    ];

    [Test, TestCaseSource(nameof(ValidationTestsProvider))]
    public void ValidationTests(CreateUserRequest request, ExpectedValidationResult expected)
    {
        var validator = new CreateUserValidator();
        var results = validator.Validate(request);
        var resultsErrorDictionary = results.ToDictionary();

        Assert.Multiple(() =>
        {
            Assert.That(results.IsValid, Is.EqualTo(expected.Result));
            Assert.That(resultsErrorDictionary, Is.EqualTo(expected.Errors));
        });
    }
}
