using System.Diagnostics;
using Application.Common;
using Application.Features.UserFeatures;
using Application.Features.UserFeatures.CreateUser;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using WebAPI.Controllers;

namespace IntegrationTests.Presentation.WebAPI.Controllers;

[SetUpFixture]
public class UserControllerTestsSetup
{
    public static PersistenceInfra StandardPersistenceInfra;
    public static PersistenceInfra NullPersistenceInfra;

    [OneTimeSetUp]
    public async Task Before()
    {
        StandardPersistenceInfra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .AddUnleashFeatureToggles()
            .AddValkeyCaching()
            .Build();
        NullPersistenceInfra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .Build();
    }

    [OneTimeTearDown]
    public async Task After()
    {
        await StandardPersistenceInfra.Dispose();
        await NullPersistenceInfra.Dispose();
    }
}

public class UserControllerTests
{
    private static object[] _postUserTestCases =
    {
        new object[]
        {
            new CreateUserRequest
            {
                FirstName = "   Isaac           ",
                LastName = "                Newton ",
                Email = "       isaac.newton@records.invalid ",
                Password = "testPassword"
            },
            201,
            new UserResult(ResultStatusTypes.Created, new UserResponse
            {
                FirstName = "Isaac",
                LastName = "Newton",
                Email = "isaac.newton@records.invalid",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            })
        },
        new object[]
        {
            new CreateUserRequest
            {
                FirstName = "Pythagoras",
                LastName = "Of Samos",
                Email = "pythagoras.of.samos@records.invalid",
                Password = "testPassword"
            },
            201,
            new UserResult(ResultStatusTypes.Created, new UserResponse
            {
                FirstName = "Pythagoras",
                LastName = "Of Samos",
                Email = "pythagoras.of.samos@records.invalid",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            })
        },
        new object[]
        {
            new CreateUserRequest
            {
                FirstName = null,
                LastName = "",
                Email = "albert.einstein@records.invalid",
                Password = "test Pa"
            },
            400,
            new UserResult(ResultStatusTypes.Created, new Dictionary<string, List<string>>
            {
                {"FirstName", ["'First Name' must not be empty."]},
                {"LastName", ["'Last Name' must not be empty."]},
                {"Password", ["The length of 'Password' must be at least 8 characters. You entered 7 characters.", "The 'Password' must not contain whitespace."]},
            })
        },
        new object[]
        {
            new CreateUserRequest
            {
                FirstName = "Albert",
                LastName = "Einstein",
                Email = "albert.einstein@records.invalid",
                Password = "testPassword"
            },
            400,
            new UserResult(ResultStatusTypes.Created, new Dictionary<string, List<string>>
            {
                {"Email", ["'Email' is already in use."]},
            })
        },
    };


    [Test]
    [TestCaseSource(nameof(_postUserTestCases))]
    public async Task PostUserTests(CreateUserRequest request,
        int expectedStatusCode, UserResult expectedResult)
    {
        await ActualPostUserTests(UserControllerTestsSetup.StandardPersistenceInfra, request, expectedStatusCode, expectedResult);
        await ActualPostUserTests(UserControllerTestsSetup.NullPersistenceInfra, request, expectedStatusCode, expectedResult);
    }

    private static async Task ActualPostUserTests(PersistenceInfra persistenceInfra, CreateUserRequest request,
        int expectedStatusCode, UserResult expectedResult)
    {
        Debug.Assert(persistenceInfra.RecordsFeatureStatus != null && persistenceInfra.CachedUserRepository != null);
        var createUserHandler = new CreateUserHandler(persistenceInfra.RecordsFeatureStatus, persistenceInfra.CachedUserRepository, persistenceInfra.Logger);
        var userController = new UserController(persistenceInfra.Logger, createUserHandler);
        var actual = await userController.Post(request);

        var actualWithStatusCode = (actual as IConvertToActionResult).Convert() as IStatusCodeActionResult;
        var actualUserResult = (actual.Result as ObjectResult)?.Value as UserResult;

        Assert.Multiple(() =>
        {
            Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));
            Assert.That(actualUserResult?.Errors, Is.EqualTo(expectedResult.Errors));

            if ((expectedResult.User == null && actualUserResult?.User != null) ||
                (expectedResult.User != null && actualUserResult?.User == null))
                Assert.Fail("expectedResult.User and actualUserResult.User does not match");

            if (expectedResult.User != null)
            {
                Assert.That(actualUserResult?.User?.FirstName, Is.EqualTo(expectedResult.User.FirstName));
                Assert.That(actualUserResult?.User?.LastName, Is.EqualTo(expectedResult.User.LastName));
                Assert.That(actualUserResult?.User?.Email, Is.EqualTo(expectedResult.User.Email));
            }
        });
    }
}
