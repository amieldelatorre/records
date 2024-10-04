using System.Diagnostics;
using Application.Configuration;
using Unleash;
using Unleash.ClientFactory;

namespace Persistence.Repositories.FeatureToggle;

public class UnleashConfiguration
{
    private static EnvironmentVariable<string> _apiUrl = new ("UNLEASH__API_URL", true);
    private static EnvironmentVariable<string> _apiKey = new ("UNLEASH__API_KEY", true);
    private static EnvironmentVariable<string> _appName = new ("UNLEASH__APP_NAME", true);

    public static IUnleash GetClient()
    {
        Serilog.Log.Logger.Information("environment variable '{envVarName}' is set to 'true'. Retrieving unleash client", FeatureToggleConfiguration.EnableFeatureToggles.Name);
        var errors = new List<string>();

        var apiUrl = EnvironmentVariable<string>.GetEnvironmentVariable(_apiUrl);
        if (string.IsNullOrWhiteSpace(apiUrl))
            errors.Add(_apiUrl.Name);

        var apiKey = EnvironmentVariable<string>.GetEnvironmentVariable(_apiKey);
        if (string.IsNullOrWhiteSpace(apiKey))
            errors.Add(_apiKey.Name);

        var appName = EnvironmentVariable<string>.GetEnvironmentVariable(_appName);
        if (string.IsNullOrWhiteSpace(appName))
            errors.Add(_appName.Name);

        if (errors.Count > 0)
            EnvironmentVariable<string>.PrintMissingEnvironmentVariablesAndExit(errors);

        Debug.Assert(apiUrl != null && apiKey != null && appName != null);

        var settings = new UnleashSettings()
        {
            AppName = appName,
            UnleashApi = new Uri(apiUrl),
            CustomHttpHeaders = new Dictionary<string, string>()
            {
                { "Authorization", apiKey }
            }
        };

        var unleashClientFactory = new UnleashClientFactory();
        // CreateClientAsync initiates the connection to the unleash server
        // Sufficient to check if unleash server is available
        var unleash = unleashClientFactory.CreateClient(settings, synchronousInitialization: true);
        return unleash;
    }
}