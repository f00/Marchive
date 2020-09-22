using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Marchive.App.IO;
using Marchive.App.Services;
using Marchive.App.Settings;
using Microsoft.Extensions.Logging;

namespace Marchive.App
{
    internal class Marchive : IMarchive
    {
        private readonly Archiver _archiver;
        private readonly UnArchiver _unArchiver;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<Marchive> _logger;
        private readonly MArchiveSettings _settings;

        public Marchive(
            Archiver archiver,
            UnArchiver unArchiver,
            IFileSystem fileSystem,
            ILogger<Marchive> logger,
            MArchiveSettings settings)
        {
            _archiver = archiver;
            _unArchiver = unArchiver;
            _fileSystem = fileSystem;
            _logger = logger;
            _settings = settings;
        }

        public void Archive(List<string> fileNames, string archiveFileName, string password = null)
        {
            var archive = _archiver.Archive(fileNames, archiveFileName);
            if (!archive.Any())
            {
                return;
            }

            if (password != null)
            {
                archive = ResolveEncryptionAlgorithm(_settings.EncryptionAlgorithm)
                    .Encrypt(archive, password);
            }

            var saveFileName = archiveFileName + Constants.FileExtensionName;
            _fileSystem.SaveFile(saveFileName, archive);

            _logger.LogInformation("Archive {filename} successfully created.", saveFileName);
        }

        public void UnArchive(string archiveFileName, string outputUnArchiveDirectory = null, string password = null)
        {
            archiveFileName = AppendFileExtensionIfNeeded(archiveFileName);

            var archive = _fileSystem.ReadAllBytes(archiveFileName);
            if (password != null)
            {
                archive = ResolveEncryptionAlgorithm(_settings.EncryptionAlgorithm).Decrypt(archive, password);
            }

            var files = _unArchiver.UnArchive(archive);

            outputUnArchiveDirectory ??= Directory.GetCurrentDirectory();

            foreach (var (filename, content) in files)
            {
                _fileSystem.SaveFile(Path.Combine(outputUnArchiveDirectory, Path.GetFileName(filename)),
                    content);

                _logger.LogInformation("{filename} successfully extracted to {outputUnArchiveDirectory}.", filename,
                    outputUnArchiveDirectory);
            }
        }

        private static IEncryptionAlgorithm ResolveEncryptionAlgorithm(EncryptionAlgorithm algorithmName) =>
            algorithmName switch
            {
                EncryptionAlgorithm.Aes => new AesEncryption(),
                _ => throw new ArgumentException(message: "invalid enum value", paramName: nameof(algorithmName))
            };

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
