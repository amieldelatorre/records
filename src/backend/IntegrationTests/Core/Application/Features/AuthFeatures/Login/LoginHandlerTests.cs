using System.Diagnostics;
using Application.Features.AuthFeatures.Login;

namespace IntegrationTests.Core.Application.Features.AuthFeatures.Login;

public class LoginHandlerTests
{
    public static PersistenceInfra StandardPersistenceInfra;

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
    
    [SetUp]
    public void SetUp()
    {
        StandardPersistenceInfra.RenewDbContext();
    }

    [Test, TestCaseSource(nameof(_loginTestCases))]
    public async Task HandlerLoginTests(LoginRequest loginRequest, LoginResponse expectedResponse)
    {
        Debug.Assert(StandardPersistenceInfra.UserRepository != null);
        var loginHandler = new LoginHandler(StandardPersistenceInfra.UserRepository);

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
                Username = "stephenhawking",
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
                Username = "fail",
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
                Username = "alberteinstein",
                Password = "password"
            },
            new LoginResponse
            {
                Success = false
            }
        }
    };
}