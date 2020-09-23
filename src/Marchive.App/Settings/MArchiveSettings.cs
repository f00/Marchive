using System.Text;
using Marchive.App.Exceptions;

namespace Marchive.App.Settings
{
    public class MArchiveSettings
    {
        /// <summary>
        /// What encoding that should be used for processing file names.
        /// </summary>
        public Encoding FileNameEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Which encryption algorithm to use, if any.<para />
        /// Note that if <see cref="T:EncryptionAlgorithmName.None"/> is selected then an <see cref="EncryptionException"/>
        /// will have to be handled if a password is provided anyway.
        /// </summary>
        public EncryptionAlgorithmName EncryptionAlgorithmName { get; set; } = EncryptionAlgorithmName.Aes;
    }
}
