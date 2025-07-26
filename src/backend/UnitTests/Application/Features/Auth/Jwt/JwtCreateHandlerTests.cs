using Application.Features.AuthFeatures.Jwt.JwtCreate;
using Domain.Entities;
using TestsCommon.Configurations;

namespace UnitTests.Application.Features.Auth.Jwt;

public class JwtCreateHandlerTests
{
    [Test]
    public void CreateToken_Success()
    {
        var testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "alberteinstein",
            Email = "albert.einstein@example.invalid",
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