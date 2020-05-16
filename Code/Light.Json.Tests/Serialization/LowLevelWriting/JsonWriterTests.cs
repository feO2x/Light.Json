using System;
using Light.Json.Serialization.LowLevelWriting;
using Xunit;

namespace Light.Json.Tests.Serialization.LowLevelWriting
{
    public abstract class JsonWriterTests<TJsonWriter>
        where TJsonWriter : struct, IJsonWriter
    {
        private TJsonWriter _writer;

        protected ref TJsonWriter Writer => ref _writer;

        protected JsonWriterTests(TJsonWriter writer) => _writer = writer;

        [Fact]
        public void WriteTrue()
        {
            Writer.WriteTrue();
            CheckResult("true");
        }

        [Fact]
        public void WriteFalse()
        {
            Writer.WriteFalse();
            CheckResult("false");
        }

        [Fact]
        public void WriteNull()
        {
            Writer.WriteNull();
            CheckResult("null");
        }

        [Fact]
        public void WriteBeginOfObject()
        {
            Writer.WriteBeginOfObject();
            CheckResult("{");
        }

        [Fact]
        public void WriteEndOfObject()
        {
            Writer.WriteEndOfObject();
            CheckResult("}");
        }

        [Fact]
        public void WriteBeginOfArray()
        {
            Writer.WriteBeginOfArray();
            CheckResult("[");
        }

        [Fact]
        public void WriteEndOfArray()
        {
            Writer.WriteEndOfArray();
            CheckResult("]");
        }

        [Fact]
        public void WriteKeyValueSeparator()
        {
            Writer.WriteKeyValueSeparator();
            CheckResult(":");
        }

        [Fact]
        public void WriteEntrySeparator()
        {
            Writer.WriteValueSeparator();
            CheckResult(",");
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("This is a really long string.")]
        [InlineData("A number: 300550152")]
        public void WriteString(string @string)
        {
            Writer.WriteString(@string.AsSpan());
            var expected = new char[@string.Length + 2];
            expected[0] = '\"';
            // ReSharper disable once UseIndexFromEndExpression
            expected[expected.Length - 1] = '\"';
            @string.CopyTo(0, expected, 1, @string.Length);
            CheckResult(new string(expected));
        }

        protected abstract void CheckResult(string expected);
    }
}