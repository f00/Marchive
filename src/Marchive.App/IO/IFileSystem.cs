namespace Marchive.App.IO
{
    internal interface IFileSystem
    {
        void SaveFile(string filename, byte[] content);
        byte[] ReadAllBytes(string filename);
    }
}