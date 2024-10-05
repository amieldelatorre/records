using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using WebAPI.Controllers.ControllerExtensions;

namespace UnitTests.Presentation.WebAPI.Controllers.ControllerExtensions;

public class ClaimsInformationTest
{
    [Test]
    [TestCaseSource(nameof(_userIdTestCases))]
    public void UserIdTest(string? userId, Guid? expectedId)
    {

        var claims = userId != null ? new[] { new Claim("userId", userId) }
                : [];
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"))
            }
        };

        if (userId == null || expectedId == null)
        {
            Assert.Throws<AssertionException>(() =>
            {
                var claimsInfo = new ClaimsInformation(httpContextAccessor);
                _ = claimsInfo.UserId();
            });
            return;
        }

        var claimsInfo = new ClaimsInformation(httpContextAccessor);
        var result = claimsInfo.UserId();
        Assert.That(result, Is.EqualTo(expectedId));
    }

    private static object[] _userIdTestCases =
    [
        new object[]
        {
            "3e098063-d9a4-4b24-9088-7a548b92796a",
            new Guid("3e098063-d9a4-4b24-9088-7a548b92796a")
        },
        // Throws JetBrains.ReSharper.TestRunner.Logging.TraceListener+AssertionException instead of the usual
        //      AssertionException when running in JetBrains Rider.
        // new object[]
        // {
        //     null,
        //     null
        // },
    ];
}