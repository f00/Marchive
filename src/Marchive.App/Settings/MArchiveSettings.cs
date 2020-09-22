using System.Text;

namespace Marchive.App.Settings
{
    public class MArchiveSettings
    {
        /// <summary>
        /// Configure behavior of archiver
        /// </summary>
        /// <param name="fileNameEncoding">Encoding of the given file names</param>
        /// <param name="encryptionAlgorithm">The encryption algorithm to use if file encryption is enabled</param>
        public MArchiveSettings(Encoding fileNameEncoding, EncryptionAlgorithm encryptionAlgorithm)
        {
            FileNameEncoding = fileNameEncoding;
            EncryptionAlgorithm = encryptionAlgorithm;
        }

        public MArchiveSettings()
        {
        }
        public Encoding FileNameEncoding { get; set; } = Encoding.UTF8;
        public EncryptionAlgorithm EncryptionAlgorithm { get; set; }
    }
}
