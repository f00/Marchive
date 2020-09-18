using System;

namespace Marchive.App.IO
{
    public interface IFileStream : IDisposable
    {
        int Read(byte[] buffer, int offset, int count);
    }
}