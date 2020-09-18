namespace Marchive.App.IO
{
    public interface IFileSystem
    {
        void SaveFile(string filename, byte[] content);
        byte[] ReadAllBytes(string filename);
    }
}