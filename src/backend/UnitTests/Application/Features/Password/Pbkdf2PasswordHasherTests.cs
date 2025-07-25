using Application.Features.Password;

namespace UnitTests.Application.Features.Password;

public class Pbkdf2PasswordHasherTests
{
    [Test]
    public void PasswordHashTest()
    {
        var hasher = new Pbkdf2PasswordHasher();
        const string testPassword = "someones1234Password&*%^";

        var passwordHashResponse = hasher.Hash(testPassword);
        var verifyPasswordResult = hasher.Verify(testPassword, passwordHashResponse.HashedPassword,
            passwordHashResponse.Salt);

        Assert.That(verifyPasswordResult, Is.True);
    }

    [Test]
    [TestCase("95D0AF80ECC3AE2A6227A2D0D7FF0F8965C54C3B84630E2DA088DB82CD19687A", "EE5245EB0796E6E8508C4CC1F2B556DA", "someones1234Password&*%^", true)]
    [TestCase("95D0AF80ECC3AE2A6227A2D0D7FF0F8965C54C3B84630E2DA088DB82CD19687A", "EE5245EB0796E6E8508C4CC1F2B556DA", "wr0ngP$ssword", false)]
    public void KnownHashTest(string knownHash, string knownSalt, string testPassword, bool expectedResult)
    {
        var hasher = new Pbkdf2PasswordHasher();
        var verifyPasswordResult = hasher.Verify(testPassword, knownHash, knownSalt);

        Assert.That(verifyPasswordResult, Is.EqualTo(expectedResult));
    }

    [Test]
    public void TestGeneratedHashesAndSaltNotEqual()
    {
        var hasher = new Pbkdf2PasswordHasher();
        const string testPassword = "someones1234Password&*%^";

        var hashResponse1 = hasher.Hash(testPassword);
        var hashResponse2 = hasher.Hash(testPassword);

        Assert.Multiple(() =>
        {
            Assert.That(hashResponse1.HashedPassword, Is.Not.EqualTo(hashResponse2.HashedPassword));
            Assert.That(hashResponse1.Salt, Is.Not.EqualTo(hashResponse2.Salt));
        });

    }
}