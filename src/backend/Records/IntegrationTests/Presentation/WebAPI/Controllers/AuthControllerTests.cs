using System.Diagnostics;
using Application.Features.AuthFeatures.Jwt.JwtCreate;
using Application.Features.AuthFeatures.Login;
using Application.Features.UserFeatures.CreateUser;
using Application.Features.UserFeatures.GetUser;
using Common.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using WebAPI.Controllers;
using WebAPI.Controllers.ControllerExtensions;

namespace IntegrationTests.Presentation.WebAPI.Controllers;

public class AuthControllerTests
{
    static PersistenceInfra StandardInfra;
    static PersistenceInfra NullInfra;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        StandardInfra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .AddUnleashFeatureToggles()
            .AddValkeyCaching()
            .Build();
        NullInfra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .Build();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await StandardInfra.Dispose();
        await NullInfra.Dispose();
    }

    private static AuthController GetAuthController(PersistenceInfra persistenceInfra)
    {
        Debug.Assert(persistenceInfra.RecordsFeatureStatus != null && persistenceInfra.CachedUserRepository != null);

        Debug.Assert(StandardInfra.CachedUserRepository != null && StandardInfra.RecordsFeatureStatus != null);
        var loginHandler = new LoginHandler(StandardInfra.CachedUserRepository);
        var jwtConfiguration = new TestJwtConfiguration();
        var jwtCreateHandler = new JwtCreateHandler(StandardInfra.RecordsFeatureStatus, loginHandler, jwtConfiguration.Config, StandardInfra.Logger);

        var authController = new AuthController(StandardInfra.Logger, jwtCreateHandler);

        return authController;
    }

    [Test]
    [TestCaseSource(nameof(_jwtCreateTestCases))]
    public async Task JwtCreateTest(LoginRequest loginRequest, int expectedStatusCode)
    {
        await ActualJwtCreateTest(StandardInfra, loginRequest, expectedStatusCode);
        await ActualJwtCreateTest(NullInfra, loginRequest, expectedStatusCode);
    }

    public async Task ActualJwtCreateTest(PersistenceInfra infra, LoginRequest loginRequest, int expectedStatusCode)
    {
        var authController = GetAuthController(infra);

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