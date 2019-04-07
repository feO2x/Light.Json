using System.Runtime.Serialization;

namespace Light.Json.Tokenization
{
    public class Utf8ParseException : DeserializationException
    {
        public Utf8ParseException(string message) : base(message) { }

        protected Utf8ParseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}