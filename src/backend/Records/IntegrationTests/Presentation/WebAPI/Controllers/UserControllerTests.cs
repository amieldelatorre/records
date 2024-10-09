using System.Diagnostics;
using System.Security.Claims;
using Application.Common;
using Application.Features.UserFeatures;
using Application.Features.UserFeatures.CreateUser;
using Application.Features.UserFeatures.DeleteUser;
using Application.Features.UserFeatures.GetUser;
using Application.Features.UserFeatures.UpdateUser;
using Application.Features.UserFeatures.UpdateUserPassword;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using WebAPI.Controllers;
using WebAPI.Controllers.ControllerExtensions;

namespace IntegrationTests.Presentation.WebAPI.Controllers;

public class UserControllerTests
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

    private static UserController GetUserController(PersistenceInfra persistenceInfra, HttpContext? httpContext = null)
    {
        Debug.Assert(persistenceInfra.RecordsFeatureStatus != null && persistenceInfra.CachedUserRepository != null);

        var httpContextAccessor = new HttpContextAccessor();
        if (httpContext != null)
            httpContextAccessor.HttpContext = httpContext;

        var claimsInformation = new ClaimsInformation(httpContextAccessor);
        var createUserHandler = new CreateUserHandler(persistenceInfra.RecordsFeatureStatus, persistenceInfra.CachedUserRepository, persistenceInfra.Logger);
        var getUserHandler = new GetUserHandler(persistenceInfra.RecordsFeatureStatus, persistenceInfra.CachedUserRepository, persistenceInfra.Logger);
        var updateUserHandler = new UpdateUserHandler(persistenceInfra.RecordsFeatureStatus, persistenceInfra.CachedUserRepository, persistenceInfra.Logger);
        var updateUserPasswordHandler = new UpdateUserPasswordHandler(persistenceInfra.RecordsFeatureStatus,
            persistenceInfra.CachedUserRepository, persistenceInfra.Logger);
        var deleteUserHandler = new DeleteUserHandler(persistenceInfra.RecordsFeatureStatus, persistenceInfra.CachedUserRepository, persistenceInfra.Logger);

        var userController = new UserController(persistenceInfra.Logger, claimsInformation, createUserHandler,
            getUserHandler, updateUserHandler, updateUserPasswordHandler, deleteUserHandler);
        return userController;
    }

    [Test, TestCaseSource(nameof(_postUserTestCases))]
    public async Task PostUserTests(CreateUserRequest request,
        int expectedStatusCode, UserResult expectedResult)
    {
        await ActualPostUserTests(StandardPersistenceInfra, request, expectedStatusCode, expectedResult);
        await ActualPostUserTests(NullPersistenceInfra, request, expectedStatusCode, expectedResult);
    }

    private static async Task ActualPostUserTests(PersistenceInfra persistenceInfra, CreateUserRequest request,
        int expectedStatusCode, UserResult expectedResult)
    {
        var userController = GetUserController(persistenceInfra);
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

            if (expectedResult.Errors.Count > 0)
            {
                Assert.That(actualUserResult?.Errors, Is.EqualTo(expectedResult.Errors));
            }
        });
    }

    [Test, TestCaseSource(nameof(_getUserTestCases))]
    public async Task GetUserTest(string guidString, int expectedStatusCode, UserResult expectedResult)
    {
        await ActualGetUserTest(StandardPersistenceInfra, guidString, expectedStatusCode, expectedResult);
        await ActualGetUserTest(NullPersistenceInfra, guidString, expectedStatusCode, expectedResult);
    }

    private static async Task ActualGetUserTest(PersistenceInfra persistenceInfra, string guidString,
        int expectedStatusCode, UserResult expectedResult)
    {
        var claims = new[] { new Claim("userId", guidString) };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        var userController = GetUserController(persistenceInfra, httpContext);

        var actual = await userController.Get();
        var actualWithStatusCode = (actual as IConvertToActionResult).Convert() as IStatusCodeActionResult;
        var actualResult = (actual.Result as ObjectResult)?.Value as UserResult;

        Assert.Multiple(() =>
        {
            Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));
            if ((expectedResult.User == null && actualResult?.User != null) ||
                (expectedResult.User != null && actualResult?.User == null))
                Assert.Fail("expectedResult.User and actualUserResult.User does not match");

            if (expectedResult.User != null)
            {
                Assert.That(actualResult?.User?.FirstName, Is.EqualTo(expectedResult.User.FirstName));
                Assert.That(actualResult?.User?.LastName, Is.EqualTo(expectedResult.User.LastName));
                Assert.That(actualResult?.User?.Email, Is.EqualTo(expectedResult.User.Email));
            }

            if (expectedResult.Errors.Count > 0)
            {
                Assert.That(actualResult?.Errors, Is.EqualTo(expectedResult.Errors));
            }
        });

    }

    [Test, TestCaseSource(nameof(_updateUserTestCases))]
    public async Task UpdateUserTest(string guidString, UpdateUserRequest request, int expectedStatusCode, UserResult expectedResult)
    {
        await ActualUpdateUserTest(StandardPersistenceInfra, guidString, request, expectedStatusCode, expectedResult);
        await ActualUpdateUserTest(NullPersistenceInfra, guidString, request, expectedStatusCode, expectedResult);
    }

    public static async Task ActualUpdateUserTest(PersistenceInfra infra, string guidString, UpdateUserRequest request, int expectedStatusCode, UserResult expectedResult)
    {
        var claims = new[] { new Claim("userId", guidString) };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        var userController = GetUserController(infra, httpContext);

        var actual = await userController.Put(request);
        var actualWithStatusCode = (actual as IConvertToActionResult).Convert() as IStatusCodeActionResult;
        var actualResult = (actual.Result as ObjectResult)?.Value as UserResult;

        Assert.Multiple(() =>
        {
            Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));
            if ((expectedResult.User == null && actualResult?.User != null) ||
                (expectedResult.User != null && actualResult?.User == null))
                Assert.Fail("expectedResult.User and actualUserResult.User does not match");

            if (expectedResult.User != null)
            {
                Assert.That(new Guid(guidString), Is.EqualTo(expectedResult.User.Id));
                Assert.That(actualResult?.User?.FirstName, Is.EqualTo(expectedResult.User.FirstName));
                Assert.That(actualResult?.User?.LastName, Is.EqualTo(expectedResult.User.LastName));
                Assert.That(actualResult?.User?.Email, Is.EqualTo(expectedResult.User.Email));
            }

            if (expectedResult.Errors.Count > 0)
            {
                Assert.That(actualResult?.Errors, Is.EqualTo(expectedResult.Errors));
            }
        });
    }

    [Test, TestCaseSource(nameof(_deleteUserTestCases))]
    public async Task DeleteUserTest(string guidString, List<string> expectedExistingGuids, int expectedStatusCode,
        UserResult expectedResult)
    {
        // Use new infra here so that it doesn't break other tests as these are destructive operations
        var standardPersistenceInfra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .AddUnleashFeatureToggles()
            .AddValkeyCaching()
            .Build();
        var nullPersistenceInfra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .Build();

        await ActualDeleteUserTest(standardPersistenceInfra, guidString, expectedExistingGuids, expectedStatusCode, expectedResult);
        await ActualDeleteUserTest(nullPersistenceInfra, guidString, expectedExistingGuids, expectedStatusCode, expectedResult);

        await standardPersistenceInfra.Dispose();
        await nullPersistenceInfra.Dispose();
    }

    public async Task ActualDeleteUserTest(PersistenceInfra infra, string guidString, List<string> expectedExistingGuids,
        int expectedStatusCode, UserResult expectedResult)
    {
        var claims = new[] { new Claim("userId", guidString) };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        var userController = GetUserController(infra, httpContext);

        var actual = await userController.Delete();
        var actualWithStatusCode = (actual as IConvertToActionResult).Convert() as IStatusCodeActionResult;
        var actualResult = (actual.Result as ObjectResult)?.Value as UserResult;

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));
            if ((expectedResult.User == null && actualResult?.User != null) ||
                (expectedResult.User != null && actualResult?.User == null))
                Assert.Fail("expectedResult.User and actualUserResult.User does not match");

            CancellationTokenSource cancellationTokenSource;

            foreach (var expectedGuid in expectedExistingGuids)
            {
                var guid = new Guid(expectedGuid);
                cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
                Debug.Assert(infra.UserRepository != null);
                var expectedExistingUser = await infra.UserRepository.Get(guid, cancellationTokenSource.Token);

                Assert.That(expectedExistingUser, Is.Not.Null);
            }

            var expectedDeletedGuid = new Guid(guidString);
            cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
            Debug.Assert(infra.UserRepository != null);
            var expectedDeletedUser = await infra.UserRepository.Get(expectedDeletedGuid, cancellationTokenSource.Token);

            Assert.That(expectedDeletedUser, Is.Null);
        });
    }

    [Test, TestCaseSource(nameof(_updateUserPasswordTestCases))]
    public async Task UpdateUserPasswordTest(string claimsGuidString, string parameterGuidString,
        UpdateUserPasswordRequest request, int expectedStatusCode, UserResult expectedResult)
    {
        // await ActualUpdateUserPasswordTest(StandardPersistenceInfra, claimsGuidString, parameterGuidString, oldPassword, newPassword, expectedStatusCode);
        await ActualUpdateUserPasswordTest(NullPersistenceInfra, claimsGuidString, parameterGuidString, request,
            expectedStatusCode, expectedResult);
    }

    public async Task ActualUpdateUserPasswordTest(PersistenceInfra infra,
        string claimsGuidString, string parameterGuidString, UpdateUserPasswordRequest request,
        int expectedStatusCode, UserResult expectedResult)
    {
        var claims = new[] { new Claim("userId", claimsGuidString) };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        var userController = GetUserController(infra, httpContext);


        var actual = await userController.PutPassword(parameterGuidString, request);
        var actualWithStatusCode = (actual as IConvertToActionResult).Convert() as IStatusCodeActionResult;
        var actualResult = (actual.Result as ObjectResult)?.Value as UserResult;

        Assert.Multiple(() =>
        {
            Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));
            Assert.That(actualResult?.Errors, Is.EqualTo(expectedResult.Errors));
        });
    }

    private static object[] _updateUserPasswordTestCases =
    [
        new object[]
        {
            "3e098063-d9a4-4b24-9088-7a548b92796a",
            "719b57e8-0a85-403e-9742-43ace59fe88d",
            new UpdateUserPasswordRequest
            {
                CurrentPassword = "password1",
                NewPassword = "newPassword",
            },
            StatusCodes.Status404NotFound,
            new UserResult(ResultStatusTypes.NotFound)
        },
        new object[]
        {
            "3e098063-d9a4-4b24-9088-7a548b92796a",
            "3e098063-d9a4-4b24-9088-7a548b92796a",
            new UpdateUserPasswordRequest
            {
                CurrentPassword = "wrongPassword",
                NewPassword = "newPassword",
            },
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.NotFound, new Dictionary<string, List<string>>
            {
                { "CurrentPassword", ["'Current Password' is incorrect."] }
            })
        },
        new object[]
        {
            "3e098063-d9a4-4b24-9088-7a548b92796a",
            "3e098063-d9a4-4b24-9088-7a548b92796a",
            new UpdateUserPasswordRequest
            {
                CurrentPassword = "",
                NewPassword = "newPassword",
            },
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.NotFound, new Dictionary<string, List<string>>
            {
                { "CurrentPassword", ["'Current Password' is incorrect."] }
            })
        },
        new object[]
        {
            "571fac2e-317c-417e-982d-be2943edb07e",
            "571fac2e-317c-417e-982d-be2943edb07e",
            new UpdateUserPasswordRequest
            {
                CurrentPassword = "password1",
                NewPassword = "newPassword",
            },
            StatusCodes.Status200OK,
            new UserResult(ResultStatusTypes.Ok)
        },
    ];

    private static object[] _deleteUserTestCases =
    [
        new object[]
        {
            "3e098063-d9a4-4b24-9088-123412312345",
            new List<string>
            {
                "3e098063-d9a4-4b24-9088-7a548b92796a",
                "719b57e8-0a85-403e-9742-43ace59fe88d",
                "3e098063-d9a4-4b24-9088-7a548b92796a"
            },
            StatusCodes.Status404NotFound,
            new UserResult(ResultStatusTypes.NotFound)
        },
        new object[]
        {
            "3e098063-d9a4-4b24-9088-7a548b92796a",
            new List<string>
            {
                "719b57e8-0a85-403e-9742-43ace59fe88d",
                "571fac2e-317c-417e-982d-be2943edb07e"
            },
            StatusCodes.Status200OK,
            new UserResult(ResultStatusTypes.Ok)
        }
    ];

    private static object[] _updateUserTestCases =
    [
        new object[]
        {
            "3e098063-d9a4-4b24-9088-7a548b92796a",
            new UpdateUserRequest
            {
                FirstName = "Stephen2",
                LastName = "Hawking2",
                Email = "stephen2.hawking2@records.invalid",
            },
            StatusCodes.Status200OK,
            new UserResult(ResultStatusTypes.Ok, new UserResponse
            {
                Id = new Guid("3e098063-d9a4-4b24-9088-7a548b92796a"),
                FirstName = "Stephen2",
                LastName = "Hawking2",
                Email = "stephen2.hawking2@records.invalid",
                DateCreated = default,
                DateUpdated = default
            })
        },
        new object[]
        {
            "571fac2e-317c-417e-982d-be2943edb07e",
            new UpdateUserRequest
            {
                FirstName = "             Albert",
                LastName = "  Einstein     ",
                Email = "albert.einstein@records.invalid",
            },
            StatusCodes.Status200OK,
            new UserResult(ResultStatusTypes.Ok, new UserResponse
            {
                Id = new Guid("571fac2e-317c-417e-982d-be2943edb07e"),
                FirstName = "Albert",
                LastName = "Einstein",
                Email = "albert.einstein@records.invalid",
                DateCreated = default,
                DateUpdated = default
            })
        },
        new object[]
        {
            "719b57e8-0a85-403e-9742-43ace59fe88d",
            new UpdateUserRequest
            {
                FirstName = "",
                LastName = "",
                Email = null,
            },
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"FirstName", ["'First Name' must not be empty."]},
                {"LastName", ["'Last Name' must not be empty."]},
                {"Email", ["'Email' must not be empty."]},
            })
        },
        new object[]
        {
            "719b57e8-0a85-403e-9742-43ace59fe88d",
            new UpdateUserRequest
            {
                FirstName = "  ",
                LastName = "",
                Email = "not an email",
            },
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"FirstName", ["'First Name' must not be empty."]},
                {"LastName", ["'Last Name' must not be empty."]},
                {"Email", ["'Email' is not a valid email address."]},
            })
        },
    ];

    private static object[] _getUserTestCases =
    [
        new object[]
        {
            "571fac2e-317c-417e-982d-be2943edb07e",
            StatusCodes.Status200OK,
            new UserResult(ResultStatusTypes.Ok, new UserResponse
            {
                Id = new Guid("571fac2e-317c-417e-982d-be2943edb07e"),
                FirstName = "Albert",
                LastName = "Einstein",
                Email = "albert.einstein@records.invalid",
                DateCreated = default,
                DateUpdated = default
            })
        },
        new object[]
        {
            "719b57e8-0a85-403e-9742-43ace59fe88d",
            StatusCodes.Status200OK,
            new UserResult(ResultStatusTypes.Ok, new UserResponse
            {
                Id = new Guid("719b57e8-0a85-403e-9742-43ace59fe88d"),
                FirstName = "Marie",
                LastName = "Curie",
                Email = "marie.curie@records.invalid",
                DateCreated = default,
                DateUpdated = default
            })
        },
        new object[]
        {
            "3e098063-d9a4-4b24-9088-7a548b92796a",
            StatusCodes.Status200OK,
            new UserResult(ResultStatusTypes.Ok, new UserResponse
            {
                Id = new Guid("3e098063-d9a4-4b24-9088-7a548b92796a"),
                FirstName = "Stephen",
                LastName = "Hawking",
                Email = "stephen.hawking@records.invalid",
                DateCreated = default,
                DateUpdated = default
            })
        },
        new object[]
        {
            "3e098063-d9a4-4b24-9088-123412312345",
            StatusCodes.Status404NotFound,
            new UserResult(ResultStatusTypes.NotFound)
        }
    ];

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
            StatusCodes.Status201Created,
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
            StatusCodes.Status201Created,
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
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
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
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"Email", ["'Email' is already in use."]},
            })
        },
    };
}
