using System.IO;

namespace Marchive.Tests.Utilities
{
    internal static class StringExtensions
    {
        public static string WithPath(this string filename, string path) =>
            Path.Combine(path, filename);
    }
}
