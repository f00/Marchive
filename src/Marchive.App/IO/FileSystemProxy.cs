using System.IO;

namespace Marchive.App.IO
{
    public class FileSystemProxy : IFileSystem
    {
        public IFileStream OpenFile(string filename) =>
            new FileStreamProxy(File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None));

        public void SaveFile(string filename, byte[] content)
        {
            var file = new FileInfo(filename);
            file.Directory?.Create();
            File.WriteAllBytes(filename, content);
        }
    }
}