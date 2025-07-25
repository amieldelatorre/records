using Application.Features.UserFeatures.UpdateUser;
using Domain.Entities;

namespace UnitTests.Application.Features.UserFeatures.UpdateUser;

public class UpdateUserMapperTests
{
    [Test, TestCaseSource(nameof(_updateUserMapperTestCases))]
    public void MapTest(UpdateUserRequest request, User oldUser, User expected)
    {
        var now = DateTime.Now;
        oldUser.DateCreated = now;
        oldUser.DateUpdated = now;
        Thread.Sleep(1000);

        UpdateUserMapper.Map(request, oldUser);
        Assert.Multiple(() =>
        {
            Assert.That(oldUser.Id, Is.EqualTo(expected.Id));
            Assert.That(oldUser.Username, Is.EqualTo(expected.Username));
            Assert.That(oldUser.Email, Is.EqualTo(expected.Email));
            Assert.That(oldUser.DateUpdated, Is.Not.EqualTo(oldUser.DateCreated));
            Assert.That(oldUser.DateCreated, Is.EqualTo(now));
        });
    }

    private static object[] _updateUserMapperTestCases =
    [
        new object[]
        {
            new UpdateUserRequest
            {
                Username = "albert2einstein2",
                Email = "albert2.einstein2@example.invalid",
            },
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
            new User
            {
                Id = new Guid("571fac2e-317c-417e-982d-be2943edb07e"),
                Username = "albert2einstein2",
                Email = "albert2.einstein2@example.invalid",
                PasswordHash = "hashedPassword",
                PasswordSalt = "passwordSalt",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            }
        },
        new object[]
        {
            new UpdateUserRequest
            {
                Username = "     albert3einstein ",
                Email =  "  albert3.einstein@example.invalid    ",
            },
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
            new User
            {
                Id = new Guid("571fac2e-317c-417e-982d-be2943edb07e"),
                Username = "albert3einstein",
                Email = "albert3.einstein@example.invalid",
                PasswordHash = "hashedPassword",
                PasswordSalt = "passwordSalt",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            }
        },
    ];
}