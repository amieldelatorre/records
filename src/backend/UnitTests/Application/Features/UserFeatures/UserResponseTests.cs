using Application.Features.UserFeatures;
using Domain.Entities;

namespace UnitTests.Application.Features.UserFeatures;

public class UserResponseTests
{
    [Test, TestCaseSource(nameof(UserResponseMapFromUserTestCases))]
    public void UserResponse_MapFromUserTests(User user, UserResponse expectedResult)
    {
        var actual = UserResponse.MapFrom(user);
        Assert.Multiple(() =>
        {
            Assert.That(actual.Id, Is.EqualTo(expectedResult.Id));
            Assert.That(actual.Username, Is.EqualTo(expectedResult.Username));
            Assert.That(actual.Email, Is.EqualTo(expectedResult.Email));
        });
    }

    internal static object[] UserResponseMapFromUserTestCases =
    [
        new object[]
        {
            new User
            {
                Id = new Guid("571fac2e-317c-417e-982d-be2943edb07e"),
                Username = "alberteinstein",
                Email = "albert.einstein@example.invalid",
                PasswordHash = "hashedPassword",
                PasswordSalt = "passwordSalt",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            },
            new UserResponse
            {
                Id = new Guid("571fac2e-317c-417e-982d-be2943edb07e"),
                Username = "alberteinstein",
                Email = "albert.einstein@example.invalid",
                DateCreated = default,
                DateUpdated = default
            }
        },
        new object[]
        {
            new User
            {
                Id = new Guid("719b57e8-0a85-403e-9742-43ace59fe88d"),
                Username = "mariecurie",
                Email = "marie.curie@example.invalid",
                PasswordHash = "hashedPassword",
                PasswordSalt = "passwordSalt",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            },
            new UserResponse
            {
                Id = new Guid("719b57e8-0a85-403e-9742-43ace59fe88d"),
                Username = "mariecurie",
                Email = "marie.curie@example.invalid",
                DateCreated = default,
                DateUpdated = default
            }
        },
        new object[]
        {
            new User
            {
                Id = new Guid("3e098063-d9a4-4b24-9088-7a548b92796a"),
                Username = "stephenhawking",
                Email = "stephen.hawking@example.invalid",
                PasswordHash = "hashedPassword",
                PasswordSalt = "passwordSalt",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            },
            new UserResponse
            {
                Id = new Guid("3e098063-d9a4-4b24-9088-7a548b92796a"),
                Username = "stephenhawking",
                Email = "stephen.hawking@example.invalid",
                DateCreated = default,
                DateUpdated = default
            }
        }
    ];
}