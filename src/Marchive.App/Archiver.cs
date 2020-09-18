using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

            // Read file content
            foreach (var filename in fileNames)
            {
                var buffer = new byte[_settings.BlockSizeBytes];

                AddFileNameToBuffer(filename, buffer);

                WriteToArchive(filename, buffer);
            }

            _fileSystem.SaveFile(archiveFileName + Constants.FileExtensionName, _archiveStream.ToArray());
            _archiveStream.Flush();
        }

        private void AddFileNameToBuffer(string filename, byte[] buffer)
        {
            // First n bytes in new file block are dedicated to filename
            if (_settings.FileNameEncoding.GetBytes(filename).Length > _settings.MaxFilenameLengthBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(filename),
                    $"Filename {filename} is too long. Maximum {_settings.MaxFilenameLengthBytes} bytes allowed.");
            }

            var filenameBuffer = new byte[_settings.MaxFilenameLengthBytes];
            _settings.FileNameEncoding.GetBytes(filename).CopyTo(filenameBuffer, 0);
            filenameBuffer.CopyTo(buffer, 0);
        }

        private void WriteToArchive(string filename, byte[] buffer)
        {
            using var fs = _fileSystem.OpenFile(filename);

            WriteFirstBlockIncludingFileName(buffer, fs);

            // Write rest of the blocks
            while (true)
            {
                buffer = new byte[_settings.BlockSizeBytes]; // Zero out the buffer
                var read = fs.Read(buffer, 0, buffer.Length);
                if (read <= 0)
                {
                    // End of file
                    _archiveStream.Write(new byte[_settings.BlockSizeBytes]); // Write empty file separator block
                    break;
                }

                _archiveStream.Write(buffer, 0, buffer.Length);
            }
        }

        private void WriteFirstBlockIncludingFileName(byte[] buffer, IFileStream fs)
        {
            // Special case first block that includes the filename
            fs.Read(buffer, _settings.MaxFilenameLengthBytes, buffer.Length - _settings.MaxFilenameLengthBytes);
            _archiveStream.Write(buffer, 0, buffer.Length);
        }

        public void Dispose()
        {
            _archiveStream?.Dispose();
        }
    }
}
