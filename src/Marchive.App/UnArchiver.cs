using System;
using System.IO;
using System.Linq;
using Marchive.App.IO;
using Marchive.App.Utilities;

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

            outputUnArchiveDirectory ??= Directory.GetCurrentDirectory();

            var archive = _fileSystem.ReadAllBytes(archiveFileName);
            var metaDataPosition =
                BitConverter.ToInt64(archive.Take(Constants.HeaderSizeBytes).ToArray(), 0);
            var metaData = archive
                .Skip((int)metaDataPosition)
                .ChunkBy(Constants.MetaBlockSizeBytes);

            foreach (var fileInfoMeta in metaData)
            {
                var filename = _settings.FileNameEncoding.GetString(fileInfoMeta
                    .Skip(Constants.MetaDataFileStartPosSizeBytes + Constants.MetaDataFileEndPosSizeBytes)
                    .TakeWhile(x => x != 0).ToArray());

                var dataStartingPos =
                    BitConverter.ToInt64(fileInfoMeta.Take(Constants.MetaDataFileStartPosSizeBytes).ToArray());

                var dataEndingPos = BitConverter.ToInt64(fileInfoMeta.Skip(Constants.MetaDataFileStartPosSizeBytes)
                    .Take(Constants.MetaDataFileStartPosSizeBytes).ToArray());

                var content = archive.Skip((int)dataStartingPos).Take((int)(dataEndingPos - dataStartingPos));

                _fileSystem.SaveFile(Path.Combine(outputUnArchiveDirectory, Path.GetFileName(filename)), content.ToArray());
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
    }
}