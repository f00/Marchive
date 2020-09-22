using System;
using System.Collections.Generic;
using System.Linq;
using Marchive.App.Settings;
using Marchive.App.Utilities;

namespace Marchive.App.Services
{
    internal class UnArchiver
    {
        private readonly MArchiveSettings _settings;

        public UnArchiver(MArchiveSettings settings = null)
        {
            _settings = settings ?? new MArchiveSettings();
        }

        public IEnumerable<(string filename, byte[] content)> UnArchive(byte[] archive)
        {
            var metaDataPosition =
                BitConverter.ToInt64(archive.Take(Constants.HeaderSizeBytes).ToArray(), 0);
            var metaData = archive
                .Skip((int)metaDataPosition)
                .ChunkBy(Constants.MetaBlockSizeBytes);

            foreach (var fileInfoMeta in metaData)
            {
                yield return ExtractFile(fileInfoMeta, archive);
            }
        }

        private (string filename, byte[] content) ExtractFile(IReadOnlyCollection<byte> fileInfoMeta, IEnumerable<byte> archive)
        {
            var filename = _settings.FileNameEncoding.GetString(fileInfoMeta
                .Skip(Constants.MetaDataFileStartPosSizeBytes + Constants.MetaDataFileEndPosSizeBytes)
                .TakeWhile(x => x != 0).ToArray());

            var dataStartingPos =
                BitConverter.ToInt64(fileInfoMeta.Take(Constants.MetaDataFileStartPosSizeBytes).ToArray());

            var dataEndingPos = BitConverter.ToInt64(fileInfoMeta.Skip(Constants.MetaDataFileStartPosSizeBytes)
                .Take(Constants.MetaDataFileStartPosSizeBytes).ToArray());

            var content = archive.Skip((int)dataStartingPos).Take((int)(dataEndingPos - dataStartingPos));

            return (filename, content.ToArray());
        }
    }
}