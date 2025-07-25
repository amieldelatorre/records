using Application.Features.UserFeatures;
using Application.Features.UserFeatures.CreateUser;
using Domain.Entities;

namespace UnitTests.Application.Features.UserFeatures.CreateUser;

public class CreateUserMapperTests
{
    [Test]
    [TestCaseSource(nameof(UserRequestToUserMapperTestCases))]
    public void UserRequestToUserMapperTest(CreateUserRequest request, User expected)
    {
        var actual = CreateUserMapper.Map(request);
        Assert.Multiple(() =>
        {
            Assert.That(actual.Username, Is.EqualTo(expected.Username));
            Assert.That(actual.Email, Is.EqualTo(expected.Email));
        });
    }

    [Test]
    public void UserToUserResponseMapperTest()
    {
        var timeNowTestCase = DateTime.UtcNow;
        var testCase = new User
        {
            Id = Guid.NewGuid(),
            Username = "alberteinstein",
            Email = "albert.einstein@example.invalid",
            PasswordHash = "hisPassword",
            PasswordSalt = "passwordSalt",
            DateCreated = timeNowTestCase,
            DateUpdated = timeNowTestCase,
        };

        var actual = UserResponse.MapFrom(testCase);

        Assert.Multiple(() =>
        {
            Assert.That(actual.Username, Is.EqualTo(testCase.Username));
            Assert.That(actual.Email, Is.EqualTo(testCase.Email));
            Assert.That(actual.DateCreated, Is.EqualTo(testCase.DateCreated));
            Assert.That(actual.DateUpdated, Is.EqualTo(testCase.DateUpdated));
            Assert.That(actual.Id, Is.EqualTo(testCase.Id));
        });
    }

    internal static object[] UserRequestToUserMapperTestCases =
    [
        new object[]
        {
            new CreateUserRequest
            {
                Username = "alberteinstein",
                Email = "albert.einstein@example.invalid",
                Password = "hisPassword"
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "alberteinstein",
                Email = "albert.einstein@example.invalid",
                PasswordHash = "hashedPassword",
                PasswordSalt = "passwordSalt",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            }
        },
        new object[]
        {
            new CreateUserRequest
            {
                Username = "   alberteinstein      ",
                Email = " albert.einstein@example.invalid         ",
                Password = "hisPassword"
            },
            new User
            {
                Id = Guid.NewGuid(),
                Username = "alberteinstein",
                Email = "albert.einstein@example.invalid",
                PasswordHash = "hashedPassword",
                PasswordSalt = "passwordSalt",
                DateCreated = DateTime.UtcNow,
                DateUpdated = DateTime.UtcNow,
            }
        },
    ];
}