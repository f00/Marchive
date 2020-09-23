using System;
using System.Runtime.Serialization;

namespace Marchive.App.Exceptions
{
    [Serializable]
    public class EncryptionException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public EncryptionException()
        {
        }

        public EncryptionException(string message) : base(message)
        {
        }

        public EncryptionException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EncryptionException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}