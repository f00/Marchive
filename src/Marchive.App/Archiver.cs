using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Marchive.App.IO;

namespace Marchive.App
{
    public class Archiver : IDisposable
    {
        private readonly IFileSystem _fileSystem;
        private readonly MemoryStream _archiveStream;
        private readonly ArchiverSettings _settings;

        public Archiver(IFileSystem fileSystem, ArchiverSettings settings = null)
        {
            _fileSystem = fileSystem;
            _archiveStream = new MemoryStream();
            _settings = settings ?? new ArchiverSettings();
        }

        /// <summary>
        /// Archives one or many files into one single .mar archive file
        /// </summary>
        /// <param name="fileNames">The names (including path) of the files to archive</param>
        /// <param name="archiveFileName">The desired name of the outputted archive file</param>
        public void Archive(List<string> fileNames, string archiveFileName)
        {
            if (!fileNames.Any())
            {
                return;
            }

            WriteHeaderBlock();

            var metaBlock = new MemoryStream();
            foreach (var fileName in fileNames)
            {
                ValidateFileNameLength(fileName);

                var metaFileInfoBlock = new MemoryStream();
                var fileNameBlock = new byte[Constants.MaxFileNameLengthBytes];
                var fileStartingIndexBlock = new byte[Constants.MetaDataFileStartPosSizeBytes];
                var fileEndingIndexBlock = new byte[Constants.MetaDataFileEndPosSizeBytes];
                
                var fileStartingIndex = _archiveStream.Position;

                var file = _fileSystem.ReadAllBytes(fileName);
                _archiveStream.Write(file);

                var fileEndingIndex = _archiveStream.Position;
                
                BitConverter.GetBytes(fileStartingIndex).CopyTo(fileStartingIndexBlock, 0);
                BitConverter.GetBytes(fileEndingIndex).CopyTo(fileEndingIndexBlock, 0);
                _settings.FileNameEncoding.GetBytes(fileName).CopyTo(fileNameBlock, 0);
                metaFileInfoBlock.Write(fileStartingIndexBlock);
                metaFileInfoBlock.Write(fileEndingIndexBlock);
                metaFileInfoBlock.Write(fileNameBlock);
                metaBlock.Write(metaFileInfoBlock.ToArray());
            }

            var metaBlockStartingPosition = _archiveStream.Position;

            _archiveStream.Write(metaBlock.ToArray());

            var archive = _archiveStream.ToArray();
            BitConverter.GetBytes(metaBlockStartingPosition).CopyTo(archive, 0);
            _fileSystem.SaveFile(archiveFileName + Constants.FileExtensionName, archive);
            _archiveStream.Flush();
        }

        private void WriteHeaderBlock()
        {
            var headerBlock = new byte[Constants.HeaderSizeBytes];
            _archiveStream.Write(headerBlock);
        }

        private void ValidateFileNameLength(string filename)
        {
            if (_settings.FileNameEncoding.GetBytes(filename).Length > Constants.MaxFileNameLengthBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(filename),
                    $"Filename {filename} is too long. Maximum {Constants.MaxFileNameLengthBytes} bytes allowed.");
            }
        }

        public void Dispose()
        {
            _archiveStream?.Dispose();
        }
    }
}
