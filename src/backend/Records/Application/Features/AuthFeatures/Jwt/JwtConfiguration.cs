using System.Security.Cryptography;
using Application.Configuration.EnvironmentVariables;
using Microsoft.IdentityModel.Tokens;

namespace Application.Features.AuthFeatures.Jwt;

public class JwtConfiguration
{
    public StringEnvironmentVariable JwtEcdsa384PrivateKey { get; set; } = new ("RECORDS__JWT_ECDSA_384_PRIVATE_KEY", true);
    public ECDsaSecurityKey? JwtEcdsa384SecurityKey { get; set; }
    public string? JwtEcdsa384Algorithm { get; set; }
    public IntEnvironmentVariable JwtTokenValidTimeSeconds { get; set; } = new ("RECORDS__JWT_VALID_TIME_SECONDS", false, 604800);
    public StringEnvironmentVariable JwtIssuer { get; set; } = new ("RECORDS__JWT_ISSUER", false, "records");
    public StringEnvironmentVariable JwtAudience { get; set; } = new ("RECORDS__JWT_AUDIENCE", false, "records");

    public static JwtConfiguration GetConfiguration()
    {
        var config = new JwtConfiguration();
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

        var signingBuilder = ECDsa.Create();
        signingBuilder.ImportFromPem(config.JwtEcdsa384PrivateKey.Value);
        var signingKey = new ECDsaSecurityKey(signingBuilder);
        config.JwtEcdsa384SecurityKey = signingKey;
        config.JwtEcdsa384Algorithm = SecurityAlgorithms.EcdsaSha384;

        return config;
    }
}