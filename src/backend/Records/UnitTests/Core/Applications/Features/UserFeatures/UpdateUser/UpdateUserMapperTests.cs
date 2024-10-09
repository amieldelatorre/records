using Application.Features.UserFeatures.UpdateUser;
using Domain.Entities;

namespace UnitTests.Core.Applications.Features.UserFeatures.UpdateUser;

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
            Assert.That(oldUser.FirstName, Is.EqualTo(expected.FirstName));
            Assert.That(oldUser.LastName, Is.EqualTo(expected.LastName));
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
                FirstName = "Albert2",
                LastName = "Einstein2",
                Email = "albert2.einstein2@records.invalid",
            },
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
            new User
            {
                Id = new Guid("571fac2e-317c-417e-982d-be2943edb07e"),
                FirstName = "Albert2",
                LastName = "Einstein2",
                Email = "albert2.einstein2@records.invalid",
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
                FirstName = "     Albert3",
                LastName = "Einstein     ",
                Email =  "  albert3.einstein@records.invalid    ",
            },
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
            new User
            {
                Id = new Guid("571fac2e-317c-417e-982d-be2943edb07e"),
                FirstName = "Albert3",
                LastName = "Einstein",
                Email = "albert3.einstein@records.invalid",
                PasswordHash = "hashedPassword",
                PasswordSalt = "passwordSalt",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            }
        },
    ];
}