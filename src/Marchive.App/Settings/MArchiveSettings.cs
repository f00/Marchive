using System.Text;

namespace Marchive.App.Settings
{
    public class MArchiveSettings
    {
        /// <summary>
        /// Configure behavior of archiver
        /// </summary>
        /// <param name="fileNameEncoding">Encoding of the given file names</param>
        /// <param name="encryptionAlgorithmName">The encryption algorithm to use if file encryption is enabled</param>
        public MArchiveSettings(Encoding fileNameEncoding, EncryptionAlgorithmName encryptionAlgorithmName)
        {
            FileNameEncoding = fileNameEncoding;
            EncryptionAlgorithmName = encryptionAlgorithmName;
        }

        public MArchiveSettings()
        {
        }
        public Encoding FileNameEncoding { get; set; } = Encoding.UTF8;
        public EncryptionAlgorithmName EncryptionAlgorithmName { get; set; } = EncryptionAlgorithmName.Aes;
    }
}
