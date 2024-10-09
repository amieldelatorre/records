using Application.Features.Password;
using Application.Features.UserFeatures.UpdateUserPassword;
using Domain.Entities;

namespace UnitTests.Core.Applications.Features.UserFeatures.UpdateUserPassword;

public class UpdateUserPasswordMapperTests
{
    [Test, TestCaseSource(nameof(_updateUserPasswordMapperTestCases))]
    public void MapTest(UpdateUserPasswordRequest request, User oldUser, User expected)
    {
        var now = DateTime.UtcNow;
        oldUser.DateCreated = now;
        oldUser.DateUpdated = now;
        Thread.Sleep(1000);

        UpdateUserPasswordMapper.Map(request, oldUser);
        var validPasswordLogin = new Pbkdf2PasswordHasher().Verify(request.NewPassword, oldUser.PasswordHash, oldUser.PasswordSalt);

        Assert.Multiple(() =>
        {
            Assert.That(oldUser.Id, Is.EqualTo(expected.Id));
            Assert.That(oldUser.FirstName, Is.EqualTo(expected.FirstName));
            Assert.That(oldUser.LastName, Is.EqualTo(expected.LastName));
            Assert.That(oldUser.Email, Is.EqualTo(expected.Email));
            Assert.That(oldUser.DateUpdated, Is.Not.EqualTo(oldUser.DateCreated));
            Assert.That(oldUser.DateCreated, Is.EqualTo(now));

            Assert.That(validPasswordLogin, Is.True);
        });
    }

    private static object[] _updateUserPasswordMapperTestCases =
    [
        new object[]
        {
            new UpdateUserPasswordRequest
            {
                CurrentPassword = "password1",
                NewPassword = "newPassword",
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
                FirstName = "Albert",
                LastName = "Einstein",
                Email = "albert.einstein@records.invalid",
                PasswordHash = "hashedPassword",
                PasswordSalt = "passwordSalt",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            }

        }
    ];
}