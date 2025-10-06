using System.Diagnostics;
using System.Security.Claims;
using Application.Common;
using Application.Features.WeightEntryFeatures;
using Application.Features.WeightEntryFeatures.CreateWeightEntry;
using Application.Features.WeightEntryFeatures.DeleteWeightEntry;
using Application.Features.WeightEntryFeatures.GetWeightEntry;
using Application.Features.WeightEntryFeatures.ListWeightEntry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using WebAPI.Controllers;
using WebAPI.Controllers.ControllerExtensions;

namespace IntegrationTests.Presentation.WebAPI.Controllers;

public static class WeightEntrySetup
{
    public static WeightEntryController GetWeightEntryController(PersistenceInfra persistenceInfra,
        HttpContext httpContext)
    {
        Debug.Assert(persistenceInfra.WeightEntryRepository != null);
        
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = httpContext
        };

        var claimsInformation = new ClaimsInformation(httpContextAccessor);
        var createWeightEntryHandler = new CreateWeightEntryHandler(persistenceInfra.WeightEntryRepository, persistenceInfra.Logger);
        var getWeightEntryHandler = new GetWeightEntryHandler(persistenceInfra.WeightEntryRepository, persistenceInfra.Logger);
        var listWeightEntryHandler = new ListWeightEntryHandler(persistenceInfra.WeightEntryRepository, persistenceInfra.Logger);
        var deleteWeightEntryHandler = new DeleteWeightEntryHandler(persistenceInfra.WeightEntryRepository, persistenceInfra.Logger);
        
        var weightEntryController = new WeightEntryController(persistenceInfra.Logger, claimsInformation, 
            createWeightEntryHandler, getWeightEntryHandler, listWeightEntryHandler, deleteWeightEntryHandler);
        weightEntryController.ControllerContext.HttpContext = httpContext;
        
        return weightEntryController;
    }
}

public class WeightEntryViewTests
{    
    private static PersistenceInfra StandardPersistenceInfra;
    private const string WeightEntryApiUrlPath = "/api/v1/weightentry";

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
    public void Setup()
    {
        StandardPersistenceInfra.RenewDbContext();
    }

    [Test, TestCaseSource(nameof(_getWeightEntryTestCases))]
    public async Task GetWeightEntryTests(int num, string userIdString, string weightEntryIdString,
        int expectedStatusCode, WeightEntryResult expectedResult)
    {
        var claims = new[] {new Claim("userId", userIdString) };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        var weightEntryController = WeightEntrySetup.GetWeightEntryController(StandardPersistenceInfra, httpContext);
        
        var actual = await weightEntryController.Get(weightEntryIdString);
        var actualWithStatusCode = (actual as IConvertToActionResult).Convert() as IStatusCodeActionResult;
        var actualResult = (actual.Result as ObjectResult)?.Value as WeightEntryResult;
        
        Assert.Multiple(() =>
        {
            Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));
            if ((expectedResult.WeightEntry == null && actualResult?.WeightEntry != null) ||
                (expectedResult.WeightEntry != null && actualResult?.WeightEntry == null))
                Assert.Fail("expectedResult.WeightEntry and actualResult.WeightEntry does not match");

            if (expectedResult.WeightEntry != null)
            {
                Assert.That(actualResult?.WeightEntry?.Value, Is.EqualTo(expectedResult.WeightEntry.Value));
                Assert.That(actualResult?.WeightEntry?.EntryDate, Is.EqualTo(expectedResult.WeightEntry.EntryDate));
                Assert.That(actualResult?.WeightEntry?.Comment, Is.EqualTo(expectedResult.WeightEntry.Comment));
                Assert.That(actualResult?.WeightEntry?.UserId, Is.EqualTo(expectedResult.WeightEntry.UserId));
                Assert.That(actualResult?.WeightEntry?.Id, Is.EqualTo(expectedResult.WeightEntry.Id));
                Assert.That(actualResult?.WeightEntry?.DateCreated, Is.EqualTo(expectedResult.WeightEntry.DateCreated));
                Assert.That(actualResult?.WeightEntry?.DateUpdated, Is.EqualTo(expectedResult.WeightEntry.DateUpdated));
            }
            Assert.That(actualResult?.Errors, Is.EqualTo(expectedResult.Errors));
        });
        
    }

    [Test, TestCaseSource(nameof(_listWeightEntriesTestCases))]
    public async Task ListWeightEntriesTests(
        int num, string userIdString, ListWeightEntryQueryParameters queryParameters, 
        int expectedStatusCode, string? expectedNext, PaginatedResult<WeightEntryResponse> expectedResult)
    {
        var expectedUserId = new Guid(userIdString);
        var claims = new[] {new Claim("userId", userIdString) };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        httpContext.Request.Path = WeightEntryApiUrlPath;
        
        var weightEntryController = WeightEntrySetup.GetWeightEntryController(StandardPersistenceInfra, httpContext);
        
        var actual = await weightEntryController.List(queryParameters);
        var actualWithStatusCode = (actual as IConvertToActionResult).Convert() as IStatusCodeActionResult;
        var actualResult = (actual.Result as ObjectResult)?.Value as PaginatedResult<WeightEntryResponse>;

        Assert.Multiple(() =>
        {
            Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));

            if (actualResult is null)
            {
                Assert.Fail("expectedResult.Result == null");
            }
            
            Assert.That(actualResult?.Errors, Is.EqualTo(expectedResult.Errors));
            Assert.That(actualResult?.Total, Is.EqualTo(expectedResult.Total));
            Assert.That(actualResult?.Limit, Is.EqualTo(expectedResult.Limit));
            Assert.That(actualResult?.Offset, Is.EqualTo(expectedResult.Offset));
            Assert.That(actualResult?.Next, Is.EqualTo(expectedResult.Next));
            Assert.That(actualResult?.Next, Is.EqualTo(expectedNext));

            foreach (var weightEntryResponse in actualResult?.Items!) // Suppress warning here, there is already a check above
            {
                Assert.That(weightEntryResponse.UserId, Is.EqualTo(expectedUserId));
            }
        });
    }
    
        
    private static object[] _getWeightEntryTestCases =
    [
        new object[]
        {
            1,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            "0199a94e-9922-72b9-b9a9-b66e8e30caa4",
            StatusCodes.Status200OK,
            new WeightEntryResult(ResultStatusTypes.Ok, new WeightEntryResponse
            {
                Value = 61.12m,
                EntryDate = new DateOnly(2010, 12, 31),
                UserId = new Guid("362c8551-0fff-47fb-9ed3-9fb39828308c"),
                Comment = null,
                Id = new Guid("0199a94e-9922-72b9-b9a9-b66e8e30caa4"),
                DateCreated = new DateTime(2025, 10, 03, 9, 02, 04, 578, 68),
                DateUpdated = new DateTime(2025, 10, 03, 9, 02, 04, 578, 68),
            }),
        },
        // Fail, doesn't exist
        new object[]
        {
            2,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            "00000000-0000-0000-0000-000000000000",
            StatusCodes.Status404NotFound,
            new WeightEntryResult(ResultStatusTypes.NotFound),
        },
        // Fail, owned by someone else
        new object[]
        {
            3,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            "0199a94e-996b-7cf1-ad15-e2b3776d6f13",
            StatusCodes.Status404NotFound,
            new WeightEntryResult(ResultStatusTypes.NotFound),
        },
        // Fail, invalid guid
        new object[]
        {
            4,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            "00000000",
            StatusCodes.Status400BadRequest,
            new WeightEntryResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"weightEntryId", ["'weightEntryId' must be a valid GUID."]},
            }),
        },
    ];

    private static object[] _listWeightEntriesTestCases = 
    [
        new object?[]
        {
            1,
            "f7fdef01-1e73-4a83-a770-4a5148a919f3", // alberteinstein, has 366 entries
            new ListWeightEntryQueryParameters
            {
                Limit = 0,
                Offset = -1,
                DateFrom = new DateOnly(2001, 6, 1),
                DateTo = new DateOnly(2000, 5, 1),
            },
            StatusCodes.Status400BadRequest,
            null,
            new PaginatedResult<WeightEntryResponse>(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"Limit", ["'Limit' must be greater than '0'."]},
                {"Offset", ["'Offset' must be greater than or equal to '0'."]},
                {"DateFilters", ["'DateTo' must be greater than or equal to 'DateFrom'."]},
            })
        },
        new object?[]
        {
            2,
            "f7fdef01-1e73-4a83-a770-4a5148a919f3", // alberteinstein, has 366 entries
            new ListWeightEntryQueryParameters
            {
                Limit = -10,
                Offset = -10,
                DateFrom = new DateOnly(2001, 6, 2),
                DateTo = new DateOnly(2001, 6, 1),
            },
            StatusCodes.Status400BadRequest,
            null,
            new PaginatedResult<WeightEntryResponse>(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"Limit", ["'Limit' must be greater than '0'."]},
                {"Offset", ["'Offset' must be greater than or equal to '0'."]},
                {"DateFilters", ["'DateTo' must be greater than or equal to 'DateFrom'."]},
            })
        },
        new object?[]
        {
            3,
            "f7fdef01-1e73-4a83-a770-4a5148a919f3", // alberteinstein, has 366 entries
            new ListWeightEntryQueryParameters
            {
                Limit = 101,
            },
            StatusCodes.Status400BadRequest,
            null,
            new PaginatedResult<WeightEntryResponse>(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"Limit", ["'Limit' must be less than or equal to '100'."]},
            })
        },
        new object?[]
        {
            4,
            "f7fdef01-1e73-4a83-a770-4a5148a919f3", // alberteinstein, has 366 entries
            new ListWeightEntryQueryParameters
            {
                Limit = 200,
            },
            StatusCodes.Status400BadRequest,
            null,
            new PaginatedResult<WeightEntryResponse>(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"Limit", ["'Limit' must be less than or equal to '100'."]},
            })
        },
        new object[]
        {
            5,
            "f7fdef01-1e73-4a83-a770-4a5148a919f3", // alberteinstein, has 366 entries
            new ListWeightEntryQueryParameters
            {
                Limit = 100,
            },
            StatusCodes.Status200OK,
            $"{WeightEntryApiUrlPath}?limit=100&offset=100",
            new PaginatedResult<WeightEntryResponse>(
                ResultStatusTypes.Ok,
                [],
                366,
                100,
                0,
                WeightEntryApiUrlPath)
        },
        new object[]
        {
            6,
            "f7fdef01-1e73-4a83-a770-4a5148a919f3", // alberteinstein, has 366 entries
            new ListWeightEntryQueryParameters
            {
                Offset = 50,
            },
            StatusCodes.Status200OK,
            $"{WeightEntryApiUrlPath}?limit=30&offset=80",
            new PaginatedResult<WeightEntryResponse>(
                ResultStatusTypes.Ok,
                [],
                366,
                30,
                50,
                WeightEntryApiUrlPath)
        },
        new object?[]
        {
            6,
            "f7fdef01-1e73-4a83-a770-4a5148a919f3", // alberteinstein, has 366 entries
            new ListWeightEntryQueryParameters
            {
                DateFrom = new DateOnly(2024, 01, 01),
                DateTo = new DateOnly(2024, 01, 01),
            },
            StatusCodes.Status200OK,
            null,
            new PaginatedResult<WeightEntryResponse>(
                ResultStatusTypes.Ok,
                [],
                1,
                30,
                0,
                WeightEntryApiUrlPath)
        },
    ];
}

public class WeightEntryModifyTests
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
    public void Setup()
    {
        StandardPersistenceInfra.RenewDbContext();
    }
    
    [Test, TestCaseSource(nameof(_postWeightEntryTestCases))]
    public async Task PostWeightEntryTests(int num, string userIdString, CreateWeightEntryRequest createWeightEntryRequest,
        int expectedStatusCode, WeightEntryResult expectedResult)
    {
        var claims = new[] {new Claim("userId", userIdString) };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        var weightEntryController = WeightEntrySetup.GetWeightEntryController(StandardPersistenceInfra, httpContext);
        
        var actual = await weightEntryController.Post(createWeightEntryRequest);
        var actualWithStatusCode = (actual as IConvertToActionResult).Convert() as IStatusCodeActionResult;
        var actualResult = (actual.Result as ObjectResult)?.Value as WeightEntryResult;
        
        Assert.Multiple(() =>
        {
            Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));
            if ((expectedResult.WeightEntry == null && actualResult?.WeightEntry != null) ||
                (expectedResult.WeightEntry != null && actualResult?.WeightEntry == null))
                Assert.Fail("expectedResult.WeightEntry and actualResult.WeightEntry does not match");

            if (expectedResult.WeightEntry != null)
            {
                Assert.That(actualResult?.WeightEntry?.EntryDate, Is.EqualTo(expectedResult.WeightEntry.EntryDate));
                Assert.That(actualResult?.WeightEntry?.Value, Is.EqualTo(expectedResult.WeightEntry.Value));
                Assert.That(actualResult?.WeightEntry?.Comment, Is.EqualTo(expectedResult.WeightEntry.Comment));
            }

            Assert.That(actualResult?.Errors, Is.EqualTo(expectedResult.Errors));
        });
    }
    
    [Test, TestCaseSource(nameof(_deleteWeightEntryTestCases))]
    public async Task DeleteWeightEntryTests(int num, string userIdString, string weightEntryIdstring,
        int expectedStatusCode, WeightEntryResult expectedResult)
    {
        var claims = new[] {new Claim("userId", userIdString) };
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims))
        };
        var weightEntryController = WeightEntrySetup.GetWeightEntryController(StandardPersistenceInfra, httpContext);
        
        var actual = await weightEntryController.Delete(weightEntryIdstring);
        var actualWithStatusCode = (actual as IConvertToActionResult).Convert() as IStatusCodeActionResult;
        var actualResult = (actual.Result as ObjectResult)?.Value as WeightEntryResult;

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));
            Assert.That(actualResult?.WeightEntry, Is.Null);
            
            // Check if weight entry was actually deleted
            if (expectedStatusCode == StatusCodes.Status200OK)
            {   
                var userId = new Guid(userIdString);
                var weightEntryId = new Guid(weightEntryIdstring);
                var cancellationToken =  new CancellationTokenSource().Token;
                Debug.Assert(StandardPersistenceInfra.WeightEntryRepository != null);
                var expectedDeletedWeightEntry = await StandardPersistenceInfra.WeightEntryRepository.Get(
                    weightEntryId, userId, cancellationToken);
                Assert.That(expectedDeletedWeightEntry, Is.Null);
            }
        });
    }
    
    private static object[] _postWeightEntryTestCases =
    [
        new object[]
        {
            1,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            new CreateWeightEntryRequest
            {
                Value = 65.3m,
                EntryDate = new DateOnly(2000, 1, 1),
            },
            StatusCodes.Status201Created,
            new WeightEntryResult(ResultStatusTypes.Created, new WeightEntryResponse
            {
                Value = 65.3m,
                EntryDate = new DateOnly(2000, 1, 1),
                UserId = new Guid("362c8551-0fff-47fb-9ed3-9fb39828308c"),
                DateCreated = default,
                DateUpdated = default,
            })
        },
        new object[]
        {
            2,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            new CreateWeightEntryRequest
            {
                Value = 65.3m,
                EntryDate = DateOnly.FromDateTime(DateTime.Today).AddDays(2),
            },
            StatusCodes.Status400BadRequest,
            new WeightEntryResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"EntryDate", ["'EntryDate' cannot be greater than the current date in UTC+14"]},
            })
        },
        new object[]
        {
            3,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            new CreateWeightEntryRequest
            {
                Value = 65.3m,
                EntryDate = new DateOnly(2024, 12, 31),
            },
            StatusCodes.Status400BadRequest,
            new WeightEntryResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"EntryDate", ["'EntryDate' caught creating duplicate entry, an entry for the date already exists. " +
                               "Only one entry per date is allowed."]},
            })
        },
        new object[]
        {
            4,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            new CreateWeightEntryRequest
            {
                Value = 0,
                EntryDate = new DateOnly(2020, 12, 31),
            },
            StatusCodes.Status400BadRequest,
            new WeightEntryResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"Value", ["'Value' must be greater than '0'."]},
            })
        },
        new object[]
        {
            5,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            new CreateWeightEntryRequest
            {
                Value = -1,
                EntryDate = new DateOnly(2020, 12, 31),
            },
            StatusCodes.Status400BadRequest,
            new WeightEntryResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"Value", ["'Value' must be greater than '0'."]},
            })
        }
    ];
    
    private static object[] _deleteWeightEntryTestCases =
    [
        new object[]
        {
            1,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            "0199a94e-9922-72b9-b9a9-b66e8e30caa4",
            StatusCodes.Status200OK,
            new WeightEntryResult(ResultStatusTypes.Ok, new WeightEntryResponse
            {
                Value = 61.12m,
                EntryDate = new DateOnly(2010, 12, 31),
                UserId = new Guid("362c8551-0fff-47fb-9ed3-9fb39828308c"),
                Comment = null,
                Id = new Guid("0199a94e-9922-72b9-b9a9-b66e8e30caa4"),
                DateCreated = new DateTime(2025, 10, 03, 9, 02, 04, 578, 68),
                DateUpdated = new DateTime(2025, 10, 03, 9, 02, 04, 578, 68),
            }),
        },
        // Fail, doesn't exist
        new object[]
        {
            2,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            "00000000-0000-0000-0000-000000000000",
            StatusCodes.Status404NotFound,
            new WeightEntryResult(ResultStatusTypes.NotFound),
        },
        // Fail, owned by someone else
        new object[]
        {
            3,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            "0199a94e-996b-7cf1-ad15-e2b3776d6f13",
            StatusCodes.Status404NotFound,
            new WeightEntryResult(ResultStatusTypes.NotFound),
        },
        // Fail, invalid guid
        new object[]
        {
            4,
            "362c8551-0fff-47fb-9ed3-9fb39828308c",
            "00000000",
            StatusCodes.Status400BadRequest,
            new WeightEntryResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>
            {
                {"weightEntryId", ["'weightEntryId' must be a valid GUID."]},
            }),
        },
    ];
}