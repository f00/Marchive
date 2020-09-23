using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Marchive.App.Exceptions;
using Marchive.App.IO;
using Marchive.App.Services;
using Marchive.App.Settings;
using Microsoft.Extensions.Logging;

namespace Marchive.App
{
    internal class Marchive : IMarchive
    {
        private const int Int32Bytes = 4;
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
                if (_settings.EncryptionAlgorithmName == EncryptionAlgorithmName.None)
                {
                    throw new EncryptionException("Password was provided but encryption is not enabled.");
                }
                var encryption = ResolveEncryptionAlgorithm(_settings.EncryptionAlgorithmName);
                archive = AddEncryptionInfoToArchive(encryption.Encrypt(archive, password), _settings.EncryptionAlgorithmName);
            }
            else
            {
                archive = AddEncryptionInfoToArchive(archive, EncryptionAlgorithmName.None);
            }

            var saveFileName = archiveFileName + Constants.FileExtensionName;
            _fileSystem.SaveFile(saveFileName, archive);

            _logger.LogInformation("Archive {filename} successfully created.", saveFileName);
        }

        private byte[] AddEncryptionInfoToArchive(byte[] archive, EncryptionAlgorithmName encryptionAlgorithmName)
        {
            var newArchive = new byte[archive.Length + Int32Bytes];
            var bytes = BitConverter.GetBytes((int)encryptionAlgorithmName);
            archive.CopyTo(newArchive, 0);
            bytes.CopyTo(newArchive, archive.Length);

            return newArchive;
        }

        public void UnArchive(string archiveFileName, string outputUnArchiveDirectory = null, string password = null)
        {
            archiveFileName = AppendFileExtensionIfNeeded(archiveFileName);

            var encryptionAlgorithmName =
                GetEncryptionAlgorithmNameFromArchive(_fileSystem.ReadAllBytes(archiveFileName),
                    out var archiveStripped);

            var archive = encryptionAlgorithmName != EncryptionAlgorithmName.None
                ? ResolveEncryptionAlgorithm(encryptionAlgorithmName).Decrypt(archiveStripped, password)
                : archiveStripped;

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

        private static string AppendFileExtensionIfNeeded(string archiveFileName)
        {
            if (!archiveFileName.EndsWith(Constants.FileExtensionName))
            {
                archiveFileName += Constants.FileExtensionName;
            }

            return archiveFileName;
        }

        private static EncryptionAlgorithmName GetEncryptionAlgorithmNameFromArchive(byte[] archive, out byte[] archiveStrippedOfEncryptionInfo)
        {
            var encryptionAlgorithm = (EncryptionAlgorithmName)BitConverter.ToInt32(archive, archive.Length - Int32Bytes);
            archiveStrippedOfEncryptionInfo = new byte[archive.Length - Int32Bytes];

            Array.Copy(archive, archiveStrippedOfEncryptionInfo, archiveStrippedOfEncryptionInfo.Length);

            return encryptionAlgorithm;
        }

        private static IEncryptionAlgorithm ResolveEncryptionAlgorithm(EncryptionAlgorithmName algorithmNameName) =>
            algorithmNameName switch
            {
                EncryptionAlgorithmName.Aes => new AesEncryption(),
                _ => throw new ArgumentException(message: "invalid enum value", paramName: nameof(algorithmNameName))
            };
    }
}
