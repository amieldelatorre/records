using WebAPI.Controllers;
using Application.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace UnitTests.Presentation.WebAPI.Controllers;

public class ControllerExtensionsTests
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
    public void HttpResponseFromResultTests(BaseResult test, int expectedStatusCode)
    {
        IConvertToActionResult actual = ControllerExtensions<BaseResult>.HttpResponseFromResult(test);
        var actualWithStatusCode = actual.Convert() as IStatusCodeActionResult;

        Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));
    }
}