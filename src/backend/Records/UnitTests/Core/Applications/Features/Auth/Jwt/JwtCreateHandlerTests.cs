using Application.Features.AuthFeatures.Jwt;
using Application.Features.AuthFeatures.Jwt.JwtCreate;
using Common.Configuration;
using Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace UnitTests.Core.Applications.Features.Auth.Jwt;

public class JwtCreateHandlerTests
{
    [Test]
    public void CreateToken_Success()
    {
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Albert",
            LastName = "Einstein",
            Email = "albert.einstein@records.invalid",
            PasswordHash = "hashedPassword",
            PasswordSalt = "passwordSalt",
            DateCreated = DateTime.UtcNow,
            DateUpdated = DateTime.UtcNow,
        };
        var jwtConfiguration = new TestJwtConfiguration();
        var result = JwtCreateHandler.CreateToken(testUser, jwtConfiguration.Config);
        Assert.That(result.AccessToken, Is.Not.Null);
    }
}