using System.Collections.Generic;

namespace Marchive.App
{
    public interface IMarchive
    {
        /// <summary>
        /// Archives one or many files into one single .mar archive file
        /// </summary>
        /// <param name="fileNames">The names (including path) of the files to archive</param>
        /// <param name="archiveFileName">The desired name of the outputted archive file</param>
        void Archive(List<string> fileNames, string archiveFileName);

        /// <summary>
        /// Extracts an existing .mar archive into the desired output directory
        /// </summary>
        /// <param name="archiveFileName">Name of the archive file (without file extension)</param>
        /// <param name="outputUnArchiveDirectory">(Optional) Name of the desired output directory in which the extracted filed will be placed</param>
        void UnArchive(string archiveFileName, string outputUnArchiveDirectory = null);
    }
}