using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Marchive.App.IO;
using Marchive.App.Settings;
using Microsoft.Extensions.Logging;

namespace Marchive.App.Services
{
    /// <summary>
    /// -------Archive file structure--------
    /// +--------+--------------+-----------+
    /// | HEADER | FILE CONTENT | META DATA |
    /// +--------+--------------+-----------+
    /// * Header contains starting position of meta data block
    /// * Meta data block contains one sub blocks per file in archive
    /// * Sub blocks contain start and end position of the file and the file name
    /// </summary>
    internal class Archiver : IDisposable
    {
        private readonly IFileSystem _fileSystem;
        private readonly MemoryStream _archiveStream;
        private readonly MemoryStream _metaDataStream;
        private readonly ILogger<Archiver> _logger;
        private readonly MArchiveSettings _settings;

        public Archiver(IFileSystem fileSystem, ILogger<Archiver> logger, MArchiveSettings settings = null)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            _archiveStream = new MemoryStream();
            _metaDataStream = new MemoryStream();
            _settings = settings ?? new MArchiveSettings();
        }

        public void Archive(List<string> fileNames, string archiveFileName)
        {
            if (!fileNames.Any())
            {
                return;
            }

            var archive = CreateArchive(fileNames);

            var saveFileName = archiveFileName + Constants.FileExtensionName;
            _fileSystem.SaveFile(saveFileName, archive);
            _archiveStream.Flush();

            _logger.LogInformation("Archive {filename} successfully created.", saveFileName);
        }

        private byte[] CreateArchive(List<string> fileNames)
        {
            WriteEmptyHeaderToArchiveStream();

            foreach (var fileName in fileNames)
            {
                ValidateFileNameLength(fileName);

                var (startPos, endPos) = WriteFileToArchiveStream(fileName);

                WriteFileMetaDataToMetaStream(fileName, startPos, endPos);
            }

            var metaBlockStartingPosition = _archiveStream.Position; // End of file data

            _archiveStream.Write(_metaDataStream.ToArray());

            var archive = UpdateArchiveHeaderWithMetaBlockStartingPosition(metaBlockStartingPosition, _archiveStream.ToArray());

            return archive;
        }

        private void WriteEmptyHeaderToArchiveStream()
        {
            var emptyHeaderBlock = new byte[Constants.HeaderSizeBytes];
            _archiveStream.Write(emptyHeaderBlock);
        }

        private void ValidateFileNameLength(string filename)
        {
            if (_settings.FileNameEncoding.GetBytes(filename).Length > Constants.MaxFileNameLengthBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(filename),
                    $"Filename {filename} is too long. Maximum {Constants.MaxFileNameLengthBytes} bytes allowed.");
            }
        }

        private (long startPos, long endPos) WriteFileToArchiveStream(string fileName)
        {
            var startPos = _archiveStream.Position;
            var file = _fileSystem.ReadAllBytes(fileName);
            _archiveStream.Write(file);

            return (startPos, startPos + file.Length);
        }

        private void WriteFileMetaDataToMetaStream(string fileName, long fileStartingIndex, long fileEndingIndex)
        {
            var metaFileInfoBlock = new MemoryStream();
            var fileNameBlock = new byte[Constants.MaxFileNameLengthBytes];
            var fileStartingIndexBlock = new byte[Constants.MetaDataFileStartPosSizeBytes];
            var fileEndingIndexBlock = new byte[Constants.MetaDataFileEndPosSizeBytes];
            BitConverter.GetBytes(fileStartingIndex).CopyTo(fileStartingIndexBlock, 0);
            BitConverter.GetBytes(fileEndingIndex).CopyTo(fileEndingIndexBlock, 0);
            _settings.FileNameEncoding.GetBytes(fileName).CopyTo(fileNameBlock, 0);
            metaFileInfoBlock.Write(fileStartingIndexBlock);
            metaFileInfoBlock.Write(fileEndingIndexBlock);
            metaFileInfoBlock.Write(fileNameBlock);

            _metaDataStream.Write(metaFileInfoBlock.ToArray());
        }

        private byte[] UpdateArchiveHeaderWithMetaBlockStartingPosition(long metaBlockStartingPosition, byte[] archive)
        {
            BitConverter.GetBytes(metaBlockStartingPosition).CopyTo(archive, 0);

            return archive;
        }

        public void Dispose()
        {
            _archiveStream?.Dispose();
            _metaDataStream?.Dispose();
        }
    }
}
