using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Marchive.App.IO;
using Marchive.App.Settings;
using Marchive.App.Utilities;
using Microsoft.Extensions.Logging;

namespace Marchive.App.Services
{
    internal class UnArchiver
    {
        private readonly IFileSystem _fileSystem;
        private readonly MArchiveSettings _settings;
        private readonly ILogger<UnArchiver> _logger;

        public UnArchiver(IFileSystem fileSystem, ILogger<UnArchiver> logger, MArchiveSettings settings = null)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            _settings = settings ?? new MArchiveSettings();
        }

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

            int successCount = 0;
            var errors = new List<string>();
            foreach (var fileInfoMeta in metaData)
            {
                try
                {
                    ExtractFile(outputUnArchiveDirectory, fileInfoMeta, archive);
                    successCount++;
                }
                catch (Exception e)
                {
                    errors.Add(e.Message);
                }
            }

            _logger.LogInformation("{success}/{total} files extracted successfully.", successCount, metaData.Count);
            if (errors.Any())
            {
                _logger.LogError("{errorCount} files with errors. Messages: {newline}{errorMessages}",
                    metaData.Count - successCount, Environment.NewLine, string.Join(Environment.NewLine, errors));
            }
        }

        private void ExtractFile(string outputUnArchiveDirectory, IReadOnlyCollection<byte> fileInfoMeta, byte[] archive)
        {
            var filename = _settings.FileNameEncoding.GetString(fileInfoMeta
                .Skip(Constants.MetaDataFileStartPosSizeBytes + Constants.MetaDataFileEndPosSizeBytes)
                .TakeWhile(x => x != 0).ToArray());

            var dataStartingPos =
                BitConverter.ToInt64(fileInfoMeta.Take(Constants.MetaDataFileStartPosSizeBytes).ToArray());

            var dataEndingPos = BitConverter.ToInt64(fileInfoMeta.Skip(Constants.MetaDataFileStartPosSizeBytes)
                .Take(Constants.MetaDataFileStartPosSizeBytes).ToArray());

            var content = archive.Skip((int) dataStartingPos).Take((int) (dataEndingPos - dataStartingPos));

            _fileSystem.SaveFile(Path.Combine(outputUnArchiveDirectory, Path.GetFileName(filename)), content.ToArray());
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