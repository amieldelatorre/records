using System.Diagnostics;
using Application.Features.AuthFeatures.Login;

namespace IntegrationTests.Core.Application.Features.Login;

public class LoginHandlerTests
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
    [TestCaseSource(nameof(_loginTestCases))]
    public async Task HandlerLoginTests(LoginRequest loginRequest, LoginResponse expectedResponse)
    {
        Debug.Assert(infra.CachedUserRepository != null);
        var loginHandler = new LoginHandler(infra.CachedUserRepository);

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(2));
        var actual = await loginHandler.Handle(loginRequest, cancellationTokenSource.Token);

        Assert.That(actual.Success, Is.EqualTo(expectedResponse.Success));
    }

    private static object[] _loginTestCases =
    {
        new object[]
        {
            new LoginRequest
            {
                Email = "stephen.hawking@records.invalid",
                Password = "password"
            },
            new LoginResponse
            {
                Success = true,
            }
        },
        new object[]
        {
            new LoginRequest
            {
                Email = "fail@records.invalid",
                Password = "fail"
            },
            new LoginResponse
            {
                Success = false
            }
        },
        new object[]
        {
            new LoginRequest
            {
                Email = "albert.einstein@records.invalid",
                Password = "password"
            },
            new LoginResponse
            {
                Success = false
            }
        }
    };
}