using System;
using System.Runtime.Serialization;

namespace Light.Json
{
    [Serializable]
    public class SerializationException : System.Runtime.Serialization.SerializationException
    {
        public SerializationException(string? message = null, Exception? innerException = null) : base(message, innerException) { }

        protected SerializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}