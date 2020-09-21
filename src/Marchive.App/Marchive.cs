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

        public Marchive(Archiver archiver, UnArchiver unArchiver, IFileSystem fileSystem, ILogger<Marchive> logger)
        {
            _archiver = archiver;
            _unArchiver = unArchiver;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        /// <summary>
        /// Archives one or many files into one single .mar archive file
        /// </summary>
        /// <param name="fileNames">The names (including path) of the files to archive</param>
        /// <param name="archiveFileName">The desired name of the outputted archive file</param>
        public void Archive(List<string> fileNames, string archiveFileName)
        {
            var archive = _archiver.Archive(fileNames, archiveFileName);
            if (!archive.Any())
            {
                return;
            }

            var saveFileName = archiveFileName + Constants.FileExtensionName;
            _fileSystem.SaveFile(saveFileName, archive);

            _logger.LogInformation("Archive {filename} successfully created.", saveFileName);
        }

        /// <summary>
        /// Extracts an existing .mar archive into the desired output directory
        /// </summary>
        /// <param name="archiveFileName">Name of the archive file (without file extension)</param>
        /// <param name="outputUnArchiveDirectory">(Optional) Name of the desired output directory in which the extracted filed will be placed</param>
        public void UnArchive(string archiveFileName, string outputUnArchiveDirectory = null)
        {
            var files = _unArchiver.UnArchive(archiveFileName);

            outputUnArchiveDirectory ??= Directory.GetCurrentDirectory();

            foreach (var (filename, content) in files)
            {
                _fileSystem.SaveFile(Path.Combine(outputUnArchiveDirectory, Path.GetFileName(filename)),
                    content);

                _logger.LogInformation("{filename} successfully extracted to {outputUnArchiveDirectory}.", filename,
                    outputUnArchiveDirectory);
            }
        }
    }
}
