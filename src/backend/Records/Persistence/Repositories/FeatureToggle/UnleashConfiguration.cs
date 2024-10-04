using System.Diagnostics;
using Application.Configuration.EnvironmentVariables;
using Unleash;
using Unleash.ClientFactory;

namespace Persistence.Repositories.FeatureToggle;

public class UnleashConfiguration
{
    private static StringEnvironmentVariable _apiUrl = new ("UNLEASH__API_URL", true);
    private static StringEnvironmentVariable _apiKey = new ("UNLEASH__API_KEY", true);
    private static StringEnvironmentVariable _appName = new ("UNLEASH__APP_NAME", true);

    public static IUnleash GetClient()
    {
        Serilog.Log.Logger.Information("environment variable '{envVarName}' is set to 'true'. Retrieving unleash client", FeatureToggleConfiguration.EnableFeatureToggles.Name);
        var errors = new List<EnvironmentVariable>();

        _apiKey.GetEnvironmentVariable();
        if (!_apiKey.IsValid)
            errors.Add(_apiKey);
        _apiUrl.GetEnvironmentVariable();
        if (!_apiUrl.IsValid)
            errors.Add(_apiUrl);
        _appName.GetEnvironmentVariable();
        if (!_appName.IsValid)
            errors.Add(_appName);

        if (errors.Count > 0)
            EnvironmentVariable.PrintMissingEnvironmentVariablesAndExit(errors);

        Debug.Assert(_apiUrl.Value != null && _apiKey.Value != null);
        var settings = new UnleashSettings()
        {
            AppName = _appName.Value,
            UnleashApi = new Uri(_apiUrl.Value),
            CustomHttpHeaders = new Dictionary<string, string>()
            {
                { "Authorization", _apiKey.Value }
            }
        };

        var unleashClientFactory = new UnleashClientFactory();
        // CreateClientAsync initiates the connection to the unleash server
        // Sufficient to check if unleash server is available
        var unleash = unleashClientFactory.CreateClient(settings, synchronousInitialization: true);
        return unleash;
    }
}