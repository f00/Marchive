namespace Marchive.App.IO
{
    public interface IFileSystem
    {
        IFileStream OpenFile(string filename);
        void SaveFile(string filename, byte[] content);
    }
}