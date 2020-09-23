using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Marchive.App.Exceptions;
using Marchive.App.IO;
using Marchive.App.Settings;
using Microsoft.Extensions.Logging;

namespace Marchive.App.Services
{
    /// <summary>
    /// Data format
    /// +---------------------------------------+
    /// | ARCHIVE DATA | ENCRYPTION INFORMATION |
    /// +---------------------------------------+
    /// </summary>
    internal class Marchive : IMarchive
    {
        private readonly Archiver _archiver;
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<Marchive> _logger;
        private readonly MArchiveSettings _settings;

        public const string FileExtensionName = ".mar";
        private const int EncryptionInformationHeaderSizeBytes = 4;

        public Marchive(
            Archiver archiver,
            IFileSystem fileSystem,
            ILogger<Marchive> logger,
            MArchiveSettings settings)
        {
            _archiver = archiver;
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

            var encryptedArchive = EncryptArchive(archive, password);

            var saveFileName = archiveFileName + FileExtensionName;
            _fileSystem.SaveFile(saveFileName, encryptedArchive);

            _logger.LogInformation("Archive {filename} successfully created.", saveFileName);
        }

        private byte[] EncryptArchive(byte[] archive, string password)
        {
            byte[] encryptedArchive;
            if (password != null)
            {
                if (_settings.EncryptionAlgorithmName == EncryptionAlgorithmName.None)
                {
                    throw new EncryptionException("Password was provided but encryption is not enabled.");
                }

                var encryptionAlgorithm = ResolveEncryptionAlgorithm(_settings.EncryptionAlgorithmName);
                encryptedArchive = AddEncryptionInfoToArchive(encryptionAlgorithm.Encrypt(archive, password),
                    _settings.EncryptionAlgorithmName);
            }
            else
            {
                encryptedArchive = AddEncryptionInfoToArchive(archive, EncryptionAlgorithmName.None);
            }

            return encryptedArchive;
        }

        private static byte[] AddEncryptionInfoToArchive(byte[] archive, EncryptionAlgorithmName encryptionAlgorithmName)
        {
            var newArchive = new byte[archive.Length + EncryptionInformationHeaderSizeBytes];
            var bytes = BitConverter.GetBytes((int)encryptionAlgorithmName);
            archive.CopyTo(newArchive, 0);
            bytes.CopyTo(newArchive, archive.Length);

            return newArchive;
        }

        public void UnArchive(string archiveFileName, string outputUnArchiveDirectory = null, string password = null)
        {
            archiveFileName = AppendFileExtensionIfNeeded(archiveFileName);

            var archive = DecryptArchive(archiveFileName, password);

            var files = _archiver.UnArchive(archive);

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
            if (!archiveFileName.EndsWith(FileExtensionName))
            {
                archiveFileName += FileExtensionName;
            }

            return archiveFileName;
        }

        private byte[] DecryptArchive(string archiveFileName, string password)
        {
            var archive = _fileSystem.ReadAllBytes(archiveFileName);
            var encryptionAlgorithmName =
                GetEncryptionAlgorithmNameFromArchive(archive, out var archiveStripped);

            var decryptedArchive = encryptionAlgorithmName != EncryptionAlgorithmName.None
                ? ResolveEncryptionAlgorithm(encryptionAlgorithmName).Decrypt(archiveStripped, password)
                : archiveStripped;

            return decryptedArchive;
        }

        private static EncryptionAlgorithmName GetEncryptionAlgorithmNameFromArchive(byte[] archive, out byte[] archiveStrippedOfEncryptionInfo)
        {
            var encryptionAlgorithm = (EncryptionAlgorithmName)BitConverter.ToInt32(archive, archive.Length - EncryptionInformationHeaderSizeBytes);
            archiveStrippedOfEncryptionInfo = new byte[archive.Length - EncryptionInformationHeaderSizeBytes];

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
