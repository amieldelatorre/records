using System.Diagnostics;
using System.Security.Claims;
using Application.Common;
using Application.Features.WeightEntryFeatures;
using Application.Features.WeightEntryFeatures.CreateWeightEntry;
using Application.Features.WeightEntryFeatures.GetWeightEntry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using WebAPI.Controllers;
using WebAPI.Controllers.ControllerExtensions;

namespace IntegrationTests.Presentation.WebAPI.Controllers;

public static class WeightEntrySetup
{
    public static WeightEntryController GetWeightEntryController(PersistenceInfra persistenceInfra,
        HttpContext? httpContext = null)
    {
        Debug.Assert(persistenceInfra.WeightEntryRepository != null);
        
        var httpContextAccessor = new HttpContextAccessor();
        if (httpContext != null)
            httpContextAccessor.HttpContext = httpContext;
        
        var claimsInformation = new ClaimsInformation(httpContextAccessor);
        var createWeightEntryHandler = new CreateWeightEntryHandler(persistenceInfra.WeightEntryRepository, persistenceInfra.Logger);
        var getWeightEntryHandler = new GetWeightEntryHandler(persistenceInfra.WeightEntryRepository, persistenceInfra.Logger);
        
        var weightEntryController = new WeightEntryController(persistenceInfra.Logger, claimsInformation, 
            createWeightEntryHandler, getWeightEntryHandler);
        
        return weightEntryController;
    }
}

public class WeightEntryViewTests
{    private static PersistenceInfra StandardPersistenceInfra;

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
        
    private static object[] _getWeightEntryTestCases =
    [
        // Success
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

    private static object[] _postWeightEntryTestCases =
    {
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
        },
    };
}