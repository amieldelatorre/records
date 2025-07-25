using System.Diagnostics;
using Application.Common;
using Application.Features.AuthFeatures.Login;
using Domain.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Application.Features.AuthFeatures.Jwt.JwtCreate;

// Essentially this works as the login handler for JWT Auth
public class JwtCreateHandler(
    LoginHandler loginHandler,
    JwtConfiguration jwtConfiguration,
    Serilog.ILogger logger)
{
    private const string FeatureName = "JwtCreate";

    public async Task<JwtCreateResult> Handle(LoginRequest request)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(
            Application.Features.SharedConfiguration.DefaultRequestTimeout));
        var loginRequestIsValid = await loginHandler.Handle(request, cancellationTokenSource.Token);
        if (!loginRequestIsValid.Success)
            return new JwtCreateResult(ResultStatusTypes.InvalidCredentials, 
                LoginHandler.GetInvalidCredentialsMessage());

        Debug.Assert(loginRequestIsValid.User != null);
        var tokenResponse = CreateToken(loginRequestIsValid.User, jwtConfiguration);
        return new JwtCreateResult(ResultStatusTypes.Created, tokenResponse);
    }

    public static JwtCreateResponse CreateToken(User user, JwtConfiguration jwtConfiguration)
    {
        var issuedTime = DateTime.UtcNow;
        var expiry = issuedTime.AddSeconds(jwtConfiguration.JwtTokenValidTimeSeconds.Value);
        var claims = new Dictionary<string, object>
        {
            ["userId"] = user.Id,
        };

        var signingCredentials = new SigningCredentials(
            jwtConfiguration.JwtEcdsa384SecurityKey, jwtConfiguration.JwtEcdsa384Algorithm);

        var tokenHandler = new JsonWebTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = jwtConfiguration.JwtIssuer.Value,
            Audience = jwtConfiguration.JwtAudience.Value,
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