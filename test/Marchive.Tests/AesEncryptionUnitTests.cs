using System;
using System.Text;
using FluentAssertions;
using Marchive.App.Exceptions;
using Marchive.App.Services;
using Xunit;

namespace Marchive.Tests
{
    public class AesEncryptionUnitTests
    {
        private readonly AesEncryption _encryptionAlgorithm = new AesEncryption();

        [Fact]
        public void GivenContentAndKey_WhenEncrypt_ThenContentIsEncrypted()
        {
            var content = Encoding.UTF8.GetBytes("This is some text");
            const string password = "OneSymmetricKey";

            var encrypted = _encryptionAlgorithm.Encrypt(content, password);

            encrypted.Should().NotEqual(content);
        }

        [Fact]
        public void GivenEncryptedContent_WhenDecrypt_WithCorrectKey_ThenContentIsDecrypted()
        {
            var content = Encoding.UTF8.GetBytes("This is some text");
            const string password = "OneSymmetricKey";
            var encrypted = _encryptionAlgorithm.Encrypt(content, password);

            var decrypted = _encryptionAlgorithm.Decrypt(encrypted, password);

            decrypted.Should().Equal(content);
        }

        [Fact]
        public void GivenEncryptedContent_WhenDecrypt_WithIncorrectKey_ThenContentIsNotDecrypted()
        {
            var content = Encoding.UTF8.GetBytes("This is some text");
            const string password = "password1";
            var encrypted = _encryptionAlgorithm.Encrypt(content, password);

            Action act = () => _encryptionAlgorithm.Decrypt(encrypted, "password2");

            act.Should().Throw<InvalidEncryptionKeyException>();
        }
    }
}
