using System.Diagnostics;
using Application.Features;

namespace IntegrationTests.Core.Application.Features;

public class FeatureStatusTests
{
    public static PersistenceInfra StandardPersistenceInfra;

    [OneTimeSetUp]
    public async Task Before()
    {
        StandardPersistenceInfra = await new PersistenceInfraBuilder()
            .AddPostgresDatabase()
            .AddUnleashFeatureToggles()
            .Build();
    }

    [OneTimeTearDown]
    public async Task After()
    {
        await StandardPersistenceInfra.Dispose();
    }

    [Test]
    [TestCase("tests_FeatureDisabled", false)]
    [TestCase("tests_FeatureEnabled", true)]
    public async Task IsFeatureEnabled(string featureName, bool expected)
    {
        Debug.Assert(StandardPersistenceInfra.FeatureToggleRepository != null &&
                     StandardPersistenceInfra.CacheRepository != null);
        var featureStatus = new FeatureStatus(StandardPersistenceInfra.FeatureToggleRepository,
            StandardPersistenceInfra.CacheRepository, StandardPersistenceInfra.Logger);

        var actual = await featureStatus.IsFeatureEnabled(featureName);
        Assert.That(actual, Is.EqualTo(expected));
    }
}