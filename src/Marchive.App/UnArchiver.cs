using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Marchive.App.IO;

namespace Marchive.App
{
    public class UnArchiver
    {
        private readonly IFileSystem _fileSystem;
        private readonly ArchiverSettings _settings;

        public UnArchiver(IFileSystem fileSystem, ArchiverSettings settings = null)
        {
            _fileSystem = fileSystem;
            _settings = settings ?? new ArchiverSettings();
        }

        /// <summary>
        /// Extracts an existing .mar archive into the desired output directory
        /// </summary>
        /// <param name="archiveFileName">Name of the archive file (without file extension)</param>
        /// <param name="outputUnArchiveDirectory">(Optional) Name of the desired output directory in which the extracted filed will be placed</param>
        public void UnArchive(string archiveFileName, string outputUnArchiveDirectory = null)
        {
            archiveFileName = AppendFileExtensionIfNeeded(archiveFileName);
            // TODO Move encoding to settings

            outputUnArchiveDirectory ??= Directory.GetCurrentDirectory();

            var rawFiles = SplitArchive(archiveFileName);

            foreach (var rawFile in rawFiles)
            {
                var filename = _settings.FileNameEncoding.GetString(rawFile.TakeWhile(x => x != 0).ToArray());
                var content = rawFile.Skip(_settings.MaxFilenameLengthBytes).ToArray();

                _fileSystem.SaveFile(Path.Combine(outputUnArchiveDirectory, Path.GetFileName(filename)), content);
            }
        }

        private static string AppendFileExtensionIfNeeded(string archiveFileName)
        {
            if (!archiveFileName.EndsWith(Constants.FileExtensionName))
            {
                archiveFileName += Constants.FileExtensionName;
            }

            return archiveFileName;
        }

        private IEnumerable<List<byte>> SplitArchive(string archiveFileName)
        {
            var rawFiles = new List<List<byte>>();
            var file = new List<byte>();
            using var fs = _fileSystem.OpenFile(archiveFileName);
            while (true)
            {
                // Split into raw list of files
                var buffer = new byte[_settings.BlockSizeBytes];
                var read = fs.Read(buffer, 0, buffer.Length);

                if (read <= 0)
                {
                    // End of file
                    break;
                }

                if (IsEmptyBlock(buffer))
                {
                    rawFiles.Add(file);
                    file = new List<byte>();
                }
                else
                {
                    file.AddRange(RemoveEmptyBytesAtEndOfBlock(buffer));
                }
            }

            return rawFiles;
        }

        private static bool IsEmptyBlock(byte[] block) => block.All(x => x == 0);

        private byte[] RemoveEmptyBytesAtEndOfBlock(byte[] data)
        {
            int lastNonZeroBitIndex = -1;
            for (var i = data.Length - 1; i > 0; i--)
            {
                if (data[i] != 0)
                {
                    lastNonZeroBitIndex = i;
                    break;
                }
            }

            var result = new byte[lastNonZeroBitIndex + 1];
            Array.Copy(data, 0, result, 0, result.Length);
            return result;
        }
    }
}