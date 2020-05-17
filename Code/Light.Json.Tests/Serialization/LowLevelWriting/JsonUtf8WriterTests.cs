using System.Text;
using FluentAssertions;
using Light.Json.FrameworkExtensions;
using Light.Json.Serialization.LowLevelWriting;
using Xunit.Abstractions;

namespace Light.Json.Tests.Serialization.LowLevelWriting
{
    public sealed class JsonUtf8WriterTests : JsonWriterTests<JsonUtf8Writer>
    {
        private readonly ITestOutputHelper _output;

        public JsonUtf8WriterTests(ITestOutputHelper output)
            : base(new JsonUtf8Writer(new ArrayPoolBufferProvider<byte>()))
        {
            _output = output;
        }

        protected override void CheckResult(string expected)
        {
            var utf8Json = Writer.ToUtf8Json().ToArray();
            var expectedUtf8 = expected.ToUtf8();
            var utf16Json = Encoding.UTF8.GetString(utf8Json);
            _output.WriteLine("Expected UTF-16: " + expected);
            _output.WriteLine("Actual UTF-16: " + utf16Json);
            utf8Json.Should().Equal(expectedUtf8);
        }
    }
}