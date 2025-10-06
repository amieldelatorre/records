using System.Diagnostics;
using Application.Features.AuthFeatures.Jwt.JwtCreate;
using Application.Features.AuthFeatures.Login;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using TestsCommon.Configurations;
using WebAPI.Controllers;

namespace IntegrationTests.Presentation.WebAPI.Controllers;

public class AuthControllerTests
{
    static PersistenceInfra StandardPersistenceInfra;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        StandardPersistenceInfra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .Build();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await StandardPersistenceInfra.Dispose();
    }

    private static AuthController GetAuthController()
    {
        Debug.Assert(StandardPersistenceInfra.UserRepository != null);
        var loginHandler = new LoginHandler(StandardPersistenceInfra.UserRepository);
        var jwtConfiguration = new TestJwtConfiguration();
        var jwtCreateHandlerLogger = new Logger<JwtCreateHandler>(new LoggerFactory());
        var jwtCreateHandler = new JwtCreateHandler(loginHandler, jwtConfiguration.Config, jwtCreateHandlerLogger);

        var authControllerLogger = new Logger<AuthController>(new LoggerFactory());
        var authController = new AuthController(authControllerLogger, jwtCreateHandler);

        return authController;
    }

    [Test, TestCaseSource(nameof(_jwtCreateTestCases))]
    public async Task JwtCreateTest(LoginRequest loginRequest, int expectedStatusCode)
    {
        var authController = GetAuthController();

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
                Username = "stephenhawking",
                Password = "password"
            },
            201
        },
        new object[]
        {
            new LoginRequest
            {
                Username = "fail",
                Password = "fail"
            },
            401
        }
    };
}