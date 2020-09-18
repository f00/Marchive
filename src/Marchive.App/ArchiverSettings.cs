using System.Text;

namespace Marchive.App
{
    public class ArchiverSettings
    {
        /// <summary>
        /// Configure behavior of archiver
        /// </summary>
        /// <param name="fileNameEncoding">Encoding of the given file names</param>
        public ArchiverSettings(Encoding fileNameEncoding)
        {
            FileNameEncoding = fileNameEncoding;
        }

        public ArchiverSettings()
        {
        }
        public Encoding FileNameEncoding { get; } = Encoding.UTF8;
    }
}
