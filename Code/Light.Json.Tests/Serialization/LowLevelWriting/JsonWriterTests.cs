﻿using System;
using System.Globalization;
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
        [InlineData("")]
        [InlineData("🌹")]
        [InlineData("🐱‍👤")]
        [InlineData("🧖‍♀️")]
        [InlineData("ह")]
        [InlineData("€")]
        [InlineData("한")]
        [InlineData("𐍈")]
        public void WriteString(string @string)
        {
            Writer.WriteString(@string.AsSpan());
            Span<char> expected = stackalloc char[@string.Length + 2];
            expected[0] = '\"';
            // ReSharper disable once UseIndexFromEndExpression
            expected[expected.Length - 1] = '\"';
            @string.AsSpan().CopyTo(expected.Slice(1));
            CheckResult(expected.ToString());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(15)]
        [InlineData(-102)]
        [InlineData(1024)]
        [InlineData(99475)]
        [InlineData(-104459)]
        [InlineData(-75_355_095)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void WriteInt32(int integer)
        {
            Writer.WriteInteger(integer);
            CheckResult(integer.ToString(CultureInfo.InvariantCulture));
        }

        [Theory]
        [InlineData(uint.MinValue)]
        [InlineData(uint.MaxValue)]
        [InlineData(1U)]
        [InlineData(294967295U)]
        public void WriteUInt32(uint integer)
        {
            Writer.WriteInteger(integer);
            CheckResult(integer.ToString(CultureInfo.InvariantCulture));
        }

        [Theory]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue)]
        [InlineData(0L)]
        [InlineData(-1L)]
        [InlineData(223578236811395807L)]
        [InlineData(-832178934500807L)]
        public void WriteInt64(long integer)
        {
            Writer.WriteInteger(integer);
            CheckResult(integer.ToString(CultureInfo.InvariantCulture));
        }

        [Theory]
        [InlineData(ulong.MinValue)]
        [InlineData(ulong.MaxValue)]
        [InlineData(1UL)]
        [InlineData(86321250909551615)]
        public void WriteUInt64(ulong integer)
        {
            Writer.WriteInteger(integer);
            CheckResult(integer.ToString(CultureInfo.InvariantCulture));
        }

        protected abstract void CheckResult(string expected);
    }
}