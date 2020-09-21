using System.Collections.Generic;
using Marchive.App.Services;

namespace Marchive.App
{
    internal class Marchive : IMarchive
    {
        private readonly Archiver _archiver;
        private readonly UnArchiver _unArchiver;

        public Marchive(Archiver archiver, UnArchiver unArchiver)
        {
            _archiver = archiver;
            _unArchiver = unArchiver;
        }

        /// <summary>
        /// Archives one or many files into one single .mar archive file
        /// </summary>
        /// <param name="fileNames">The names (including path) of the files to archive</param>
        /// <param name="archiveFileName">The desired name of the outputted archive file</param>
        public void Archive(List<string> fileNames, string archiveFileName) =>
            _archiver.Archive(fileNames, archiveFileName);

        /// <summary>
        /// Extracts an existing .mar archive into the desired output directory
        /// </summary>
        /// <param name="archiveFileName">Name of the archive file (without file extension)</param>
        /// <param name="outputUnArchiveDirectory">(Optional) Name of the desired output directory in which the extracted filed will be placed</param>
        public void UnArchive(string archiveFileName, string outputUnArchiveDirectory = null) =>
            _unArchiver.UnArchive(archiveFileName, outputUnArchiveDirectory);
    }
}
