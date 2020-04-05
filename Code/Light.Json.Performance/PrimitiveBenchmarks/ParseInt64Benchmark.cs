using System;
using BenchmarkDotNet.Attributes;
using Light.Json.FrameworkExtensions;
using Light.Json.PrimitiveParsing;
using DotnetUtf8Parser = System.Buffers.Text.Utf8Parser;

namespace Light.Json.Performance.PrimitiveBenchmarks
{
    public class ParseInt64Benchmark
    {
        [ParamsSource(nameof(SerializedValues))]
        public SerializedInt64 SerializedValue;

        public static SerializedInt64[] SerializedValues =>
            new[]
            {
                new SerializedInt64(long.MaxValue),
                new SerializedInt64("922337203685477580, \"otherKey\": \"some other value\" }"),
                new SerializedInt64(long.MinValue),
                new SerializedInt64("-184467440737095516, 12994, -394929121, 3040130, 19392]"),
                new SerializedInt64(1000L),
                new SerializedInt64(42),
                new SerializedInt64(0L)
            };

        [Benchmark(Baseline = true)]
        public long Utf8Parser()
        {
            if (!DotnetUtf8Parser.TryParse(SerializedValue.NumberInUtf8, out long value, out _))
                throw new InvalidOperationException();
            return value;
        }

        [Benchmark]
        public long LightJsonUtf8()
        {
            if (IntegerParser.TryParseInt64(SerializedValue.NumberInUtf8, out var value, out _) != IntegerParseResult.ParsingSuccessful)
                throw new InvalidOperationException();
            return value;
        }

        [Benchmark]
        public long LightJsonUtf16()
        {
            if (SerializedValue.NumberInUtf16.AsSpan().TryParseInt64(out var value, out _) != IntegerParseResult.ParsingSuccessful)
                throw new InvalidOperationException();
            return value;
        }

        public readonly struct SerializedInt64
        {
            public readonly string NumberInUtf16;
            public readonly byte[] NumberInUtf8;

            public SerializedInt64(long number)
            {
                NumberInUtf16 = number.ToString();
                NumberInUtf8 = NumberInUtf16.ToUtf8();
            }

            public SerializedInt64(string serializedValue)
            {
                NumberInUtf16 = serializedValue;
                NumberInUtf8 = NumberInUtf16.ToUtf8();
            }

            public override string ToString()
            {
                return NumberInUtf16;
            }
        }
    }
}