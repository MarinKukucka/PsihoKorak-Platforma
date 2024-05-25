using FluentAssertions;
using PsihoKorak_Platforma.Utils;

namespace PsihoKorak_Platforma.Tests.Utils
{
    public class CryptoUtilsTests
    {
        [Fact]
        public void GetSalt_ReturnsNonEmptyArray()
        {
            var salt = CryptoUtils.GetSalt();

            salt.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void HashPassword_GeneratesValidHash()
        {
            var password = "password123";
            var salt = CryptoUtils.GetSalt();

            var hashedPassword = CryptoUtils.HashPassword(password, salt);

            hashedPassword.Should().NotBeNullOrEmpty();
            hashedPassword.Length.Should().Be(44);
        }

        [Fact]
        public void HashPassword_ReturnsDifferentHashForDifferentSalt()
        {
            var password = "password123";
            var salt1 = CryptoUtils.GetSalt();
            var salt2 = CryptoUtils.GetSalt();

            var hashedPassword1 = CryptoUtils.HashPassword(password, salt1);
            var hashedPassword2 = CryptoUtils.HashPassword(password, salt2);

            hashedPassword1.Should().NotBe(hashedPassword2);
        }
    }
}
