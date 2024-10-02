using Common;

namespace IntegrationTests.Presentation.WebAPI.Controllers;

public class UserControllerTests
{
    private static PersistenceInfra _standardPersistenceInfra;
    private static PersistenceInfra _nullPersistenceInfra;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _standardPersistenceInfra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .AddUnleashFeatureToggles()
            .AddValkeyCaching()
            .Build();
        _nullPersistenceInfra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .Build();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _standardPersistenceInfra.Dispose();
        await _nullPersistenceInfra.Dispose();
    }

    private static object[] _postUserTestCases =
    {
        new object[]
        {
            _standardPersistenceInfra
        },
    };


    [Test]
    [TestCaseSource(nameof(_postUserTestCases))]
    public async Task PostUserTests(PersistenceInfra persistenceInfra)
    {
        await Task.CompletedTask;
    }

}