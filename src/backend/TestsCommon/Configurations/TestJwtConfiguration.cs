using System.Security.Cryptography;
using Application.Features.AuthFeatures.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace TestsCommon.Configurations;

public class TestJwtConfiguration
{
    public JwtConfiguration Config { get; set; }

    public TestJwtConfiguration()
    {
        Config = new ();
        Config.JwtIssuer.Value = "test";
        Config.JwtAudience.Value = "test";
        Config.JwtEcdsa384PrivateKey.Value = "-----BEGIN EC PRIVATE KEY-----\nMIGkAgEBBDA7BMwW8Yz2zbTi9GJfyPZBKjiALmLjNmNZ+4ga/6AV3UqDV0aLnTum\nVqATSX9YfDWgBwYFK4EEACKhZANiAATLn8hZCaSub4Vl4f7L7Xc4I0haPz9vL8ue\no3vL6zEdmaOf8zRWY7wYXKcFMVwuAs6noLldgswOfoh9clQZTqXfJNYRPQ8KjPNs\nCULqAn3AdHSlMgteG8fa/408rZWKo+Y=\n-----END EC PRIVATE KEY-----\n";
        Config.JwtTokenValidTimeSeconds.Value = 604800;
        Config.JwtEcdsa384Algorithm = SecurityAlgorithms.EcdsaSha384;

        var signingBuilder = ECDsa.Create();
        signingBuilder.ImportFromPem(Config.JwtEcdsa384PrivateKey.Value);
        var signingKey = new ECDsaSecurityKey(signingBuilder);

        Config.JwtEcdsa384SecurityKey = signingKey;
    }
}