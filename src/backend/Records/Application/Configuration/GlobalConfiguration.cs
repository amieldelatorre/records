using System.Diagnostics;
using Application.Configuration.EnvironmentVariables;

namespace Application.Configuration;

public class GlobalConfiguration
{
    public StringEnvironmentVariable JwtEcdsa384PrivateKey { get; set; } = new ("RECORDS__JWT_ECDSA_384_PRIVATE_KEY", true);
    public IntEnvironmentVariable JwtTokenValidTimeSeconds { get; set; } = new ("RECORDS__JWT_VALID_TIME_SECONDS", false, 604800);
    public StringEnvironmentVariable JwtIssuer { get; set; } = new ("RECORDS__JWT_ISSUER", false, "records");
    public StringEnvironmentVariable JwtAudience { get; set; } = new ("RECORDS__JWT_AUDIENCE", false, "records");

    public static GlobalConfiguration GetGlobalConfiguration()
    {
        var config = new GlobalConfiguration();
        var errors = new List<EnvironmentVariable>();

        config.JwtEcdsa384PrivateKey.GetEnvironmentVariable();
        if (!config.JwtEcdsa384PrivateKey.IsValid)
            errors.Add(config.JwtEcdsa384PrivateKey);
        config.JwtTokenValidTimeSeconds.GetEnvironmentVariable();
        if (!config.JwtTokenValidTimeSeconds.IsValid)
            errors.Add(config.JwtTokenValidTimeSeconds);
        config.JwtIssuer.GetEnvironmentVariable();
        if (!config.JwtIssuer.IsValid)
            errors.Add(config.JwtIssuer);
        config.JwtAudience.GetEnvironmentVariable();
        if (!config.JwtAudience.IsValid)
            errors.Add(config.JwtAudience);

        if (errors.Count > 0)
            EnvironmentVariable.PrintMissingEnvironmentVariablesAndExit(errors);

        return config;
    }
}