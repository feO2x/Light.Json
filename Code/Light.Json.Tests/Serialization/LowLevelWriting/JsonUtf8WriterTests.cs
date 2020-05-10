using System.Text;
using FluentAssertions;
using Light.Json.Serialization.LowLevelWriting;

namespace Light.Json.Tests.Serialization.LowLevelWriting
{
    public sealed class JsonUtf8WriterTests : JsonWriterTests<JsonUtf8Writer>
    {
        public JsonUtf8WriterTests() : base(new JsonUtf8Writer(new ArrayPoolBufferProvider<byte>())) { }

        protected override void CheckResult(string expected)
        {
            var utf8Json = Writer.ToUtf8Json().ToArray();
            var utf16Json = Encoding.UTF8.GetString(utf8Json);
            utf16Json.Should().Be(expected);
        }
    }
}