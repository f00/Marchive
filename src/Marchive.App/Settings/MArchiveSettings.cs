using System.Text;

namespace Marchive.App.Settings
{
    public class MArchiveSettings
    {
        /// <summary>
        /// Configure behavior of archiver
        /// </summary>
        /// <param name="fileNameEncoding">Encoding of the given file names</param>
        public MArchiveSettings(Encoding fileNameEncoding)
        {
            FileNameEncoding = fileNameEncoding;
        }

        public MArchiveSettings()
        {
        }
        public Encoding FileNameEncoding { get; } = Encoding.UTF8;
    }
}
