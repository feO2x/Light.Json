﻿using System;
using FluentAssertions;
using Light.Json.Serialization.LowLevelWriting;

namespace Light.Json.Tests.Serialization.LowLevelWriting
{
    public sealed class JsonUtf16WriterTests : JsonWriterTests<JsonUtf16Writer, char>
    {
        public JsonUtf16WriterTests() : base(new JsonUtf16Writer(Array.AsMemory())) { }

        protected override void CheckResult(string expected) =>
            Writer.ToUtf16Json().ToString().Should().Be(expected);
    }
}