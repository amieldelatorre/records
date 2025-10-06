using System.Diagnostics;
using System.Security.Claims;
using Application.Common;
using Application.Features.AuthFeatures.Jwt.JwtCreate;
using Application.Features.PasswordFeatures;
using Application.Features.UserFeatures;
using Application.Features.UserFeatures.CreateUser;
using Application.Features.UserFeatures.DeleteUser;
using Application.Features.UserFeatures.GetUser;
using Application.Features.UserFeatures.UpdateUser;
using Application.Features.UserFeatures.UpdateUserPassword;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using WebAPI.Controllers;
using WebAPI.Controllers.ControllerExtensions;

namespace IntegrationTests.Presentation.WebAPI.Controllers;

public class UserControllerTests
{
    private static PersistenceInfra StandardPersistenceInfra;
    
    [OneTimeSetUp]
    public async Task Before()
    {
        StandardPersistenceInfra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .Build();
    }
    
    [OneTimeTearDown]
    public async Task After()
    {
        await StandardPersistenceInfra.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        StandardPersistenceInfra.RenewDbContext();
    }
    
    private static UserController GetUserController(PersistenceInfra persistenceInfra, HttpContext? httpContext = null)
    {
        Debug.Assert(persistenceInfra.UserRepository != null);

        var httpContextAccessor = new HttpContextAccessor();
        if (httpContext != null)
            httpContextAccessor.HttpContext = httpContext;

        var claimsInformation = new ClaimsInformation(httpContextAccessor);
        var createUserHandlerLogger = new Logger<CreateUserHandler>(new LoggerFactory());
        var createUserHandler = new CreateUserHandler(persistenceInfra.UserRepository, createUserHandlerLogger);
        
        var getUserHandlerLogger = new Logger<GetUserHandler>(new LoggerFactory());
        var getUserHandler = new GetUserHandler(persistenceInfra.UserRepository, getUserHandlerLogger);
        
        var updateUserHandlerLogger = new Logger<UpdateUserHandler>(new LoggerFactory());
        var updateUserHandler = new UpdateUserHandler(persistenceInfra.UserRepository,updateUserHandlerLogger);
        
        var updateUserPasswordHandlerLogger = new Logger<UpdateUserPasswordHandler>(new LoggerFactory());
        var updateUserPasswordHandler = new UpdateUserPasswordHandler(persistenceInfra.UserRepository, 
            updateUserPasswordHandlerLogger);
        
        var deleteUserHandlerLogger = new Logger<DeleteUserHandler>(new LoggerFactory());
        var deleteUserHandler = new DeleteUserHandler(persistenceInfra.UserRepository, deleteUserHandlerLogger);

        var userControllerLogger = new Logger<UserController>(new LoggerFactory());
        var userController = new UserController(userControllerLogger, claimsInformation, createUserHandler,
            getUserHandler, updateUserHandler, updateUserPasswordHandler, deleteUserHandler);
        return userController;
    }

    [Test, TestCaseSource(nameof(_postUserTestCases))]
    public async Task PostUserTests(int num, CreateUserRequest request,
        int expectedStatusCode, UserResult expectedResult)
    {
        var userController = GetUserController(StandardPersistenceInfra);
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
                Assert.That(actualUserResult?.User?.Username, Is.EqualTo(expectedResult.User.Username));
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
        var claims = new[] { new Claim("userId", guidString) };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        var userController = GetUserController(StandardPersistenceInfra, httpContext);

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
                Assert.That(actualResult?.User?.Username, Is.EqualTo(expectedResult.User.Username));
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
        var claims = new[] { new Claim("userId", guidString) };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        var userController = GetUserController(StandardPersistenceInfra, httpContext);

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
                Assert.That(actualResult?.User?.Username, Is.EqualTo(expectedResult.User.Username));
                Assert.That(actualResult?.User?.Email, Is.EqualTo(expectedResult.User.Email));
            }

            if (expectedResult.Errors.Count > 0)
            {
                Assert.That(actualResult?.Errors, Is.EqualTo(expectedResult.Errors));
            }
        });
    }
    
    [Test, TestCaseSource(nameof(_updateUserPasswordTestCases))]
    public async Task UpdateUserPasswordTest(string claimsGuidString, string parameterGuidString,
        UpdateUserPasswordRequest request, int expectedStatusCode, UserResult expectedResult)
    {
        var claims = new[] { new Claim("userId", claimsGuidString) };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        var userController = GetUserController(StandardPersistenceInfra, httpContext);


        var actual = await userController.PutPassword(parameterGuidString, request);
        var actualWithStatusCode = (actual as IConvertToActionResult).Convert() as IStatusCodeActionResult;
        var actualResult = (actual.Result as ObjectResult)?.Value as UserResult;

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));
            Assert.That(actualResult?.Errors, Is.EqualTo(expectedResult.Errors));

            if (expectedStatusCode == StatusCodes.Status200OK)
            {
                Debug.Assert(StandardPersistenceInfra.UserRepository != null);
                var user = await StandardPersistenceInfra.UserRepository.Get(new Guid(claimsGuidString), new CancellationTokenSource().Token);
                Debug.Assert(user != null);
                var validNewPasswordLogin =
                    new Pbkdf2PasswordHasher().Verify(request.NewPassword, user.PasswordHash, user.PasswordSalt);
                var invalidOldPasswordLogin =
                    new Pbkdf2PasswordHasher().Verify(request.CurrentPassword, user.PasswordHash, user.PasswordSalt);

                Assert.That(validNewPasswordLogin, Is.True);
                Assert.That(invalidOldPasswordLogin, Is.False);
            }
        });
    }
    
    [Test, TestCaseSource(nameof(_deleteUserTestCases))]
    public async Task DeleteUserTest(string guidString, List<string> expectedExistingGuids, int expectedStatusCode,
        UserResult expectedResult)
    {
        // Use new infra here so that it doesn't break other tests as these are destructive operations
        var infra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .Build();
        
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
        
        await infra.Dispose();
    }
    
     private static object[] _postUserTestCases =
    {
        new object[]
        {
            1,
            new CreateUserRequest
            {
                Username = "isaacnewton",
                Email = "       isaac.newton@example.invalid ",
                Password = "testPassword"
            },
            StatusCodes.Status201Created,
            new UserResult(ResultStatusTypes.Created, new UserResponse
            {
                Username = "isaacnewton",
                Email = "isaac.newton@example.invalid",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            })
        },
        new object[]
        {
            2,
            new CreateUserRequest
            {
                Username = "pythagorasofsamos",
                Email = "pythagoras.of.samos@example.invalid",
                Password = "testPassword"
            },
            StatusCodes.Status201Created,
            new UserResult(ResultStatusTypes.Created, new UserResponse
            {
                Username = "pythagorasofsamos",
                Email = "pythagoras.of.samos@example.invalid",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            })
        },
        new object[]
        {
            3,
            new CreateUserRequest
            {
                Username = null,
                Email = "albert.einstein@example.invalid",
                Password = "test Pa"
            },
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"Username", ["'Username' must not be empty."]},
                {"Email", ["'Email' provided is already in use."]},
                {"Password", ["The length of 'Password' must be at least 8 characters. You entered 7 characters.", "The 'Password' must not contain whitespace."]},
            })
        },
        new object[]
        {
            4,
            new CreateUserRequest
            {
                Username = "alberteinstein",
                Email = "albert.einstein@example.invalid",
                Password = "testPassword"
            },
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"Username", ["'Username' provided is already in use."]},
                {"Email", ["'Email' provided is already in use."]},
            })
        },
        new object[]
        {
            5,
            new CreateUserRequest
            {
                Username = "  x  ",
                Email = "albert2.einstein@example.invalid",
                Password = "testPassword"
            },
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            { 
                {"Username", ["'Username' can only contain lowercase letters and numbers."]},
            })
        },
        new object[]
        {
            6,
            new CreateUserRequest
            {
                Username = "abc",
                Email = "albert2.einstein@example.invalid",
                Password = "testPassword"
            },
            StatusCodes.Status201Created,
            new UserResult(ResultStatusTypes.Created, new UserResponse
            {
                Username = "abc",
                Email = "albert2.einstein@example.invalid",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow
            })
        },
        new object[]
        {
            7,
            new CreateUserRequest
            {
                Username = "aa",
                Email = "albert3.einstein@example.invalid",
                Password = "testPassword"
            },
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            { 
                {"Username", ["The length of 'Username' must be at least 3 characters. You entered 2 characters."]},
            })
        },
        new object[]
        {
            8,
            new CreateUserRequest
            {
                Username = "aa!",
                Email = "albert3.einstein@example.invalid",
                Password = "testPassword"
            },
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            { 
                {"Username", ["'Username' can only contain lowercase letters and numbers."]},
            })
        }
    };
     
    private static object[] _getUserTestCases =
    [
        new object[]
        {
            "f7fdef01-1e73-4a83-a770-4a5148a919f3",
            StatusCodes.Status200OK,
            new UserResult(ResultStatusTypes.Ok, new UserResponse
            {
                Id = new Guid("f7fdef01-1e73-4a83-a770-4a5148a919f3"),
                Username = "alberteinstein",
                Email = "albert.einstein@example.invalid",
                DateCreated = default,
                DateUpdated = default
            })
        },
        new object[]
        {
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            StatusCodes.Status200OK,
            new UserResult(ResultStatusTypes.Ok, new UserResponse
            {
                Id = new Guid("362c8551-0fff-47fb-9ed3-9fb39828308c"),
                Username = "mariecurie",
                Email = "marie.curie@example.invalid",
                DateCreated = default,
                DateUpdated = default
            })
        },
        new object[]
        {
            "b378ee12-e261-47ff-8a8d-b202787bc631",
            StatusCodes.Status200OK,
            new UserResult(ResultStatusTypes.Ok, new UserResponse
            {
                Id = new Guid("b378ee12-e261-47ff-8a8d-b202787bc631"),
                Username = "stephenhawking",
                Email = "stephen.hawking@example.invalid",
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
    
    private static object[] _updateUserTestCases =
    [
        new object[]
        {
            "b378ee12-e261-47ff-8a8d-b202787bc631",
            new UpdateUserRequest
            {
                Username = "stephen2hawking2",
                Email = "stephen2.hawking2@example.invalid",
            },
            StatusCodes.Status200OK,
            new UserResult(ResultStatusTypes.Ok, new UserResponse
            {
                Id = new Guid("b378ee12-e261-47ff-8a8d-b202787bc631"),
                Username = "stephen2hawking2",
                Email = "stephen2.hawking2@example.invalid",
                DateCreated = default,
                DateUpdated = default
            })
        },
        new object[]
        {
            "f7fdef01-1e73-4a83-a770-4a5148a919f3",
            new UpdateUserRequest
            {
                Username = "   albert2einstein   ",
                Email = "albert15.einstein@example.invalid",
            },
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"Username", ["'Username' can only contain lowercase letters and numbers."]},
            })
        },
        new object[]
        {
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            new UpdateUserRequest
            {
                Username = "",
                Email = null,
            },
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"Email", ["'Email' must not be empty."]},
                {"Username", ["'Username' must not be empty.", "The length of 'Username' must be at least 3 characters. You entered 0 characters."]},
            })
        },
        new object[]
        {
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            new UpdateUserRequest
            {
                Username = "       ",
                Email = "not an email",
            },
            StatusCodes.Status400BadRequest,
            new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"Username", ["'Username' must not be empty.", "'Username' can only contain lowercase letters and numbers."]},
                {"Email", ["'Email' is not a valid email address."]},
            })
        },
         new object[]
         {
             "f7fdef01-1e73-4a83-a770-4a5148a919f3",
             new UpdateUserRequest
             {
                 Username = "mariecurie",
                 Email = "marie.curie@example.invalid",
             },
             StatusCodes.Status400BadRequest,
             new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
             {
                 {"Username", ["'Username' provided is already in use."]},
                 {"Email", ["'Email' provided is already in use."]},
             })
         },
         new object[]
         {
             "f7fdef01-1e73-4a83-a770-4a5148a919f3",
             new UpdateUserRequest
             {
                 Username = "22",
                 Email = "marie2.curie@example.invalid",
             },
             StatusCodes.Status400BadRequest,
             new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
             {
                 {"Username", ["The length of 'Username' must be at least 3 characters. You entered 2 characters."]},
             })
         },
         new object[]
         {
             "f7fdef01-1e73-4a83-a770-4a5148a919f3",
             new UpdateUserRequest
             {
                 Username = "!aa",
                 Email = "marie2.curie@example.invalid",
             },
             StatusCodes.Status400BadRequest,
             new UserResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
             {
                 {"Username", ["'Username' can only contain lowercase letters and numbers."]},
             }) 
         }
     ];

    private static object[] _updateUserPasswordTestCases =
    [
        new object[]
        {
            "b378ee12-e261-47ff-8a8d-b202787bc631",
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
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
            "b378ee12-e261-47ff-8a8d-b202787bc631",
            "b378ee12-e261-47ff-8a8d-b202787bc631",
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
            "b378ee12-e261-47ff-8a8d-b202787bc631",
            "b378ee12-e261-47ff-8a8d-b202787bc631",
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
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            new UpdateUserPasswordRequest
            {
                CurrentPassword = "password123214",
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
                "b378ee12-e261-47ff-8a8d-b202787bc631",
                "362c8551-0fff-47fb-9ed3-9fb39828308c",
                "b378ee12-e261-47ff-8a8d-b202787bc631"
            },
            StatusCodes.Status404NotFound,
            new UserResult(ResultStatusTypes.NotFound)
        },
        new object[]
        {
            "b378ee12-e261-47ff-8a8d-b202787bc631",
            new List<string>
            {
                "362c8551-0fff-47fb-9ed3-9fb39828308c",
                "f7fdef01-1e73-4a83-a770-4a5148a919f3"
            },
            StatusCodes.Status200OK,
            new UserResult(ResultStatusTypes.Ok)
        }
    ];
}