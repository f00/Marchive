using System;
using System.Runtime.Serialization;

namespace Marchive.App.Exceptions
{
    [Serializable]
    public class InvalidEncryptionKeyException : EncryptionException
    {
        public InvalidEncryptionKeyException()
        {
        }

        public InvalidEncryptionKeyException(string message) : base(message)
        {
        }

        public InvalidEncryptionKeyException(string message, Exception inner) : base(message, inner)
        {
        }

        protected InvalidEncryptionKeyException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
