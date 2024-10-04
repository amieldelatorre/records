using System.Diagnostics;
using Application.Features.AuthFeatures.Jwt.JwtCreate;
using Application.Features.AuthFeatures.Login;
using Common.Configuration;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using WebAPI.Controllers;

namespace IntegrationTests.Presentation.WebAPI.Controllers;

public class AuthControllerTests
{
    public static PersistenceInfra infra;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        infra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .AddUnleashFeatureToggles()
            .AddValkeyCaching()
            .Build();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await infra.Dispose();
    }

    [Test]
    [TestCaseSource(nameof(_jwtCreateTestCases))]
    public async Task JwtCreateTest(LoginRequest loginRequest, int expectedStatusCode)
    {
        Debug.Assert(infra.CachedUserRepository != null && infra.RecordsFeatureStatus != null);
        var loginHandler = new LoginHandler(infra.CachedUserRepository);
        var jwtConfiguration = new TestJwtConfiguration();
        var jwtCreateHandler = new JwtCreateHandler(infra.RecordsFeatureStatus, loginHandler, jwtConfiguration.Config, infra.Logger);
        var authController = new AuthController(infra.Logger, jwtCreateHandler);

        var actual = await authController.JwtCreate(loginRequest);
        var actualWithStatusCode = (actual as IConvertToActionResult).Convert() as IStatusCodeActionResult;

        Assert.That(actualWithStatusCode?.StatusCode, Is.EqualTo(expectedStatusCode));
    }


    private static object[] _jwtCreateTestCases =
    {
        new object[]
        {
            new LoginRequest
            {
                Email = "stephen.hawking@records.invalid",
                Password = "password"
            },
            201
        },
        new object[]
        {
            new LoginRequest
            {
                Email = "fail@records.invalid",
                Password = "fail"
            },
            401
        }
    };
}