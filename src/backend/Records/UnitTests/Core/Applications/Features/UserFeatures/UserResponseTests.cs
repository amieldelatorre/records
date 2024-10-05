using Application.Features.UserFeatures;
using Domain.Entities;

namespace UnitTests.Core.Applications.Features.UserFeatures;

public class UserResponseTests
{
    [Test]
    [TestCaseSource(nameof(UserResponseMapFromUserTestCases))]
    public void UserResponse_MapFromUserTests(User user, UserResponse expectedResult)
    {
        var actual = UserResponse.MapFrom(user);
        Assert.Multiple(() =>
        {
            Assert.That(actual.Id, Is.EqualTo(expectedResult.Id));
            Assert.That(actual.FirstName, Is.EqualTo(expectedResult.FirstName));
            Assert.That(actual.LastName, Is.EqualTo(expectedResult.LastName));
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
                FirstName = "Albert",
                LastName = "Einstein",
                Email = "albert.einstein@records.invalid",
                PasswordHash = "hashedPassword",
                PasswordSalt = "passwordSalt",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            },
            new UserResponse
            {
                Id = new Guid("571fac2e-317c-417e-982d-be2943edb07e"),
                FirstName = "Albert",
                LastName = "Einstein",
                Email = "albert.einstein@records.invalid",
                DateCreated = default,
                DateUpdated = default
            }
        },
        new object[]
        {
            new User
            {
                Id = new Guid("719b57e8-0a85-403e-9742-43ace59fe88d"),
                FirstName = "Marie",
                LastName = "Curie",
                Email = "marie.curie@records.invalid",
                PasswordHash = "hashedPassword",
                PasswordSalt = "passwordSalt",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            },
            new UserResponse
            {
                Id = new Guid("719b57e8-0a85-403e-9742-43ace59fe88d"),
                FirstName = "Marie",
                LastName = "Curie",
                Email = "marie.curie@records.invalid",
                DateCreated = default,
                DateUpdated = default
            }
        },
        new object[]
        {
            new User
            {
                Id = new Guid("3e098063-d9a4-4b24-9088-7a548b92796a"),
                FirstName = "Stephen",
                LastName = "Hawking",
                Email = "stephen.hawking@records.invalid",
                PasswordHash = "hashedPassword",
                PasswordSalt = "passwordSalt",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            },
            new UserResponse
            {
                Id = new Guid("3e098063-d9a4-4b24-9088-7a548b92796a"),
                FirstName = "Stephen",
                LastName = "Hawking",
                Email = "stephen.hawking@records.invalid",
                DateCreated = default,
                DateUpdated = default
            }
        }
    ];
}