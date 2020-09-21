namespace Marchive.App.Settings
{
    /// <summary>
    /// Warning, changing values here might make existing archive files unusable.
    /// (Can be fixed by writing the values to the archive files)
    /// </summary>
    internal static class Constants
    {
        public const string FileExtensionName = ".mar";
        public const int HeaderSizeBytes = 8;
        public const int MetaDataFileStartPosSizeBytes = 8;
        public const int MetaDataFileEndPosSizeBytes = 8;
        public const int MaxFileNameLengthBytes = 496;

        public static int MetaBlockSizeBytes =>
            MetaDataFileStartPosSizeBytes + MetaDataFileEndPosSizeBytes + MaxFileNameLengthBytes;
    }
}
