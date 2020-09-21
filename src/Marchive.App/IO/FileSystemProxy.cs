using System.IO;

namespace Marchive.App.IO
{
    internal class FileSystemProxy : IFileSystem
    {
        public void SaveFile(string filename, byte[] content)
        {
            var file = new FileInfo(filename);
            file.Directory?.Create();
            File.WriteAllBytes(filename, content);
        }

        public byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);
    }
}