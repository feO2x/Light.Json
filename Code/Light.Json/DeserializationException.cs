using System;
using System.Runtime.Serialization;

namespace Light.Json
{
    [Serializable]
    public class DeserializationException : Exception
    {
        public DeserializationException(string message, Exception innerException = null) : base(message, innerException) { }

        protected DeserializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}