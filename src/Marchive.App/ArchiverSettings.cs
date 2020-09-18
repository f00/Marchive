using System;
using System.Text;

namespace Marchive.App
{
    public class ArchiverSettings
    {
        /// <summary>
        /// Configure behavior of archiver
        /// </summary>
        /// <param name="blockSizeBytes">Size of blocks used in internal buffer</param>
        /// <param name="maxFilenameLengthBytes">Maximum allowed file name length in bytes (NOTE! Must be less than or equal to block size)</param>
        /// <param name="fileNameEncoding">Encoding of the given file names</param>
        public ArchiverSettings(int blockSizeBytes, int maxFilenameLengthBytes, Encoding fileNameEncoding)
        {
            if (maxFilenameLengthBytes > blockSizeBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(maxFilenameLengthBytes),
                    $"Must be less than or equal to {nameof(blockSizeBytes)}");
            }
            BlockSizeBytes = blockSizeBytes;
            MaxFilenameLengthBytes = maxFilenameLengthBytes;
            FileNameEncoding = fileNameEncoding;
        }

        public ArchiverSettings()
        {
        }

        public int BlockSizeBytes { get; } = 512;
        public int MaxFilenameLengthBytes { get; } = 100;
        public Encoding FileNameEncoding { get; } = Encoding.UTF8;
    }
}
