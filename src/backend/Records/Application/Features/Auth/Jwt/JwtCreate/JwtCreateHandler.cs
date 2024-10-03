using System.Security.Cryptography;
using Application.Common;
using Application.Features.Auth.Login;
using Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Application.Features.Auth.Jwt.JwtCreate;

// Essentially this works as the login handler for JWT Auth
public class JwtCreateHandler(
    FeatureStatus featureStatus,
    LoginHandler loginHandler,
    Serilog.ILogger logger)
{
    private const string FeatureName = "CreateUser";
    private const int TokenValidTimeSeconds = 604800; // One week

    public async Task<JwtCreateResult> Handle(LoginRequest request, CancellationToken cancellationToken)
    {
        if (!(await featureStatus.IsFeatureEnabled(FeatureName)))
        {
            logger.Debug("feature '{featureName}' is not enabled. Skipping feature and returning early", FeatureName);
            return new JwtCreateResult(ResultStatusTypes.FeatureDisabled, FeatureStatus.GetFeatureDisabledMessage());
        }

        var loginRequestIsValid = await loginHandler.Handle(request, cancellationToken);
        if (!loginRequestIsValid)
            return new JwtCreateResult(ResultStatusTypes.InvalidCredentials, LoginHandler.GetInvalidCredentialsMessage());

        var expiry= DateTime.UtcNow.AddSeconds(TokenValidTimeSeconds);
        var issuedTime = DateTime.UtcNow;
        // var tokenResponse = CreateToken(user, issuedTime, expiry);
        throw new NotImplementedException();
    }

    public static JwtCreateResponse CreateToken(User user, DateTime issuedTime, DateTime expiry)
    {

        // TODO: get signing key set up as environment variables
        var signingKey = RSA.Create();
        signingKey.ImportFromPem("envVar");
        var signingCredentials = new SigningCredentials(new RsaSecurityKey(signingKey), SecurityAlgorithms.RsaSha512);
        var tokenHandler = new JsonWebTokenHandler();

        var claims = new Dictionary<string, object>
        {
            ["userId"] = user.Id,
        };

        var descriptor = new SecurityTokenDescriptor
        {
            // TODO: Get these properly
            Issuer = "Me",
            Audience = "Them",
            Claims = claims,
            IssuedAt = issuedTime,
            NotBefore = issuedTime,
            Expires = expiry,
            SigningCredentials = signingCredentials,
        };

        var accessToken = tokenHandler.CreateToken(descriptor);
        return new JwtCreateResponse
        {
            AccessToken = accessToken,
        };
    }
}