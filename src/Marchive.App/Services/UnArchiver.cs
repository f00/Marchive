using System;
using System.Collections.Generic;
using System.Linq;
using Marchive.App.IO;
using Marchive.App.Settings;
using Marchive.App.Utilities;

namespace Marchive.App.Services
{
    internal class UnArchiver
    {
        private readonly IFileSystem _fileSystem;
        private readonly MArchiveSettings _settings;

        public UnArchiver(IFileSystem fileSystem, MArchiveSettings settings = null)
        {
            _fileSystem = fileSystem;
            _settings = settings ?? new MArchiveSettings();
        }

        public IEnumerable<(string filename, byte[] content)> UnArchive(string archiveFileName)
        {
            archiveFileName = AppendFileExtensionIfNeeded(archiveFileName);

            var archive = _fileSystem.ReadAllBytes(archiveFileName);
            var metaDataPosition =
                BitConverter.ToInt64(archive.Take(Constants.HeaderSizeBytes).ToArray(), 0);
            var metaData = archive
                .Skip((int)metaDataPosition)
                .ChunkBy(Constants.MetaBlockSizeBytes);

            foreach (var fileInfoMeta in metaData)
            {
                yield return ExtractFile(fileInfoMeta, archive);
            }
        }

        private (string filename, byte[] content) ExtractFile(IReadOnlyCollection<byte> fileInfoMeta, IEnumerable<byte> archive)
        {
            var filename = _settings.FileNameEncoding.GetString(fileInfoMeta
                .Skip(Constants.MetaDataFileStartPosSizeBytes + Constants.MetaDataFileEndPosSizeBytes)
                .TakeWhile(x => x != 0).ToArray());

            var dataStartingPos =
                BitConverter.ToInt64(fileInfoMeta.Take(Constants.MetaDataFileStartPosSizeBytes).ToArray());

            var dataEndingPos = BitConverter.ToInt64(fileInfoMeta.Skip(Constants.MetaDataFileStartPosSizeBytes)
                .Take(Constants.MetaDataFileStartPosSizeBytes).ToArray());

            var content = archive.Skip((int) dataStartingPos).Take((int) (dataEndingPos - dataStartingPos));

            return (filename, content.ToArray());
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