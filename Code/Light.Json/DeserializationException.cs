using System;
using System.Runtime.Serialization;

namespace Light.Json
{
    [Serializable]
    public class DeserializationException : Exception
    {
        public DeserializationException(string message) : base(message) { }

        protected DeserializationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}