using Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using WebAPI.Controllers.ControllerExtensions;

namespace UnitTests.Presentation.WebAPI.Controllers.ControllerExtensions;

public class HttpResponseFromResultTests
{
    internal static object[] HttpResponseFromResultTestsProvider =
    [
        new object[]
        {
            new BaseResult(ResultStatusTypes.Created, new Dictionary<string, List<string>>()),
            StatusCodes.Status201Created,
        },
        new object[]
        {
            new BaseResult(ResultStatusTypes.Ok, new Dictionary<string, List<string>>()),
            StatusCodes.Status200OK
        },
        new object[]
        {
            new BaseResult(ResultStatusTypes.FeatureDisabled, new Dictionary<string, List<string>>()),
            StatusCodes.Status403Forbidden
        },
        new object[]
        {
            new BaseResult(ResultStatusTypes.InvalidCredentials, new Dictionary<string, List<string>>()),
            StatusCodes.Status401Unauthorized
        },
        new object[]
        {
            new BaseResult(ResultStatusTypes.NotFound, new Dictionary<string, List<string>>()),
            StatusCodes.Status404NotFound
        },
        new object[]
        {
            new BaseResult(ResultStatusTypes.ValidationError, new Dictionary<string, List<string>>()),
            StatusCodes.Status400BadRequest
        },
        new object[]
        {
            new BaseResult(ResultStatusTypes.ServerError, new Dictionary<string, List<string>>()),
            StatusCodes.Status500InternalServerError
        },
        new object[]
        {
            new BaseResult((ResultStatusTypes) 1000, new Dictionary<string, List<string>>()),
            StatusCodes.Status500InternalServerError
        },
    ];

    [Test, TestCaseSource(nameof(HttpResponseFromResultTestsProvider))]
    public void MapResponseFromResultTests(BaseResult test, int expectedStatusCode)
    {
        IConvertToActionResult actual = HttpResponseFromResult<BaseResult>.Map(test);
        var actualWithStatusCode = actual.Convert() as IStatusCodeActionResult;

        Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));
    }
}