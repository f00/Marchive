using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Marchive.App.IO;
using Marchive.App.Settings;
using Marchive.App.Utilities;

namespace Marchive.App.Services
{
    /// <summary>
    /// -------Archive file structure--------
    /// +--------+--------------+-----------+
    /// | HEADER | FILE CONTENT | META DATA |
    /// +--------+--------------+-----------+
    /// * HEADER contains starting position of meta data block
    /// * META DATA contains one sub blocks per file in archive
    /// * Sub blocks contain start and end position of the file and the file name
    /// </summary>
    internal class Archiver : IDisposable
    {
        private readonly IFileSystem _fileSystem;
        private readonly MemoryStream _archiveStream;
        private readonly MemoryStream _metaDataStream;
        private readonly MArchiveSettings _settings;

        public const int HeaderSizeBytes = 8;
        public const int MetaDataFileStartPosSizeBytes = 8;
        public const int MetaDataFileEndPosSizeBytes = 8;
        public const int MaxFileNameLengthBytes = 496;

        public static int MetaBlockSizeBytes => MetaDataFileStartPosSizeBytes + MetaDataFileEndPosSizeBytes + MaxFileNameLengthBytes;

        public Archiver(IFileSystem fileSystem, MArchiveSettings settings = null)
        {
            _fileSystem = fileSystem;
            _archiveStream = new MemoryStream();
            _metaDataStream = new MemoryStream();
            _settings = settings ?? new MArchiveSettings();
        }

        public byte[] Archive(List<string> fileNames, string archiveFileName)
        {
            if (!fileNames.Any())
            {
                return new byte[0];
            }

            var archive = CreateArchive(fileNames);

            _archiveStream.Flush();

            return archive;
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
            var emptyHeaderBlock = new byte[HeaderSizeBytes];
            _archiveStream.Write(emptyHeaderBlock);
        }

        private void ValidateFileNameLength(string filename)
        {
            if (_settings.FileNameEncoding.GetBytes(filename).Length > MaxFileNameLengthBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(filename),
                    $"Filename {filename} is too long. Maximum {MaxFileNameLengthBytes} bytes allowed.");
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
            var fileNameBlock = new byte[MaxFileNameLengthBytes];
            var fileStartingIndexBlock = new byte[MetaDataFileStartPosSizeBytes];
            var fileEndingIndexBlock = new byte[MetaDataFileEndPosSizeBytes];
            BitConverter.GetBytes(fileStartingIndex).CopyTo(fileStartingIndexBlock, 0);
            BitConverter.GetBytes(fileEndingIndex).CopyTo(fileEndingIndexBlock, 0);
            _settings.FileNameEncoding.GetBytes(fileName).CopyTo(fileNameBlock, 0);
            metaFileInfoBlock.Write(fileStartingIndexBlock);
            metaFileInfoBlock.Write(fileEndingIndexBlock);
            metaFileInfoBlock.Write(fileNameBlock);

            _metaDataStream.Write(metaFileInfoBlock.ToArray());
        }

        private static byte[] UpdateArchiveHeaderWithMetaBlockStartingPosition(long metaBlockStartingPosition, byte[] archive)
        {
            BitConverter.GetBytes(metaBlockStartingPosition).CopyTo(archive, 0);

            return archive;
        }

        public IEnumerable<(string filename, byte[] content)> UnArchive(byte[] archive)
        {
            var metaDataPosition =
                BitConverter.ToInt64(archive.Take(HeaderSizeBytes).ToArray(), 0);
            var metaData = archive
                .Skip((int)metaDataPosition)
                .ChunkBy(MetaBlockSizeBytes);

            foreach (var fileInfoMeta in metaData)
            {
                yield return ExtractFile(fileInfoMeta, archive);
            }
        }

        private (string filename, byte[] content) ExtractFile(IReadOnlyCollection<byte> fileInfoMeta, IEnumerable<byte> archive)
        {
            var filename = _settings.FileNameEncoding.GetString(fileInfoMeta
                .Skip(MetaDataFileStartPosSizeBytes + MetaDataFileEndPosSizeBytes)
                .TakeWhile(x => x != 0).ToArray());

            var dataStartingPos =
                BitConverter.ToInt64(fileInfoMeta.Take(MetaDataFileStartPosSizeBytes).ToArray());

            var dataEndingPos = BitConverter.ToInt64(fileInfoMeta.Skip(MetaDataFileStartPosSizeBytes)
                .Take(MetaDataFileStartPosSizeBytes).ToArray());

            var content = archive.Skip((int)dataStartingPos).Take((int)(dataEndingPos - dataStartingPos));

            return (filename, content.ToArray());
        }

        public void Dispose()
        {
            _archiveStream?.Dispose();
            _metaDataStream?.Dispose();
        }
    }
}
