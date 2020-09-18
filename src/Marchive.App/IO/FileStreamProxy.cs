namespace Marchive.App.IO
{
    public class FileStreamProxy : IFileStream
    {
        private readonly System.IO.Stream _ioStream;

        public FileStreamProxy(System.IO.Stream ioStream)
        {
            _ioStream = ioStream;
        }

        public int Read(byte[] buffer, int offset, int count)
            => _ioStream.Read(buffer, offset, count);

        public void Dispose()
        {
            _ioStream?.Dispose();
        }
    }
}