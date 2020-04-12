using System;
using BenchmarkDotNet.Attributes;
using Light.Json.Deserialization.PrimitiveParsing;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Performance.PrimitiveBenchmarks
{
    public class ParseInt32Benchmark
    {
        [ParamsSource(nameof(SerializedValues))]
        public SerializedInt32 SerializedValue;

        public static SerializedInt32[] SerializedValues =>
            new[]
            {
                new SerializedInt32(int.MaxValue),
                new SerializedInt32("2147483647, \"otherKey\": \"some other value\" }"),
                new SerializedInt32(int.MinValue),
                new SerializedInt32("-20301923, 12994, -394929121, 3040130, 19392]"),
                new SerializedInt32(1000),
                new SerializedInt32(42),
                new SerializedInt32(0)
            };

        [Benchmark(Baseline = true)]
        public long Utf8Parser()
        {
            if (!System.Buffers.Text.Utf8Parser.TryParse(SerializedValue.NumberInUtf8, out int value, out _))
                throw new InvalidOperationException();
            return value;
        }

        [Benchmark]
        public long LightJsonUtf8()
        {
            if (IntegerParser.TryParseInt32(SerializedValue.NumberInUtf8, out var value, out _) != IntegerParseResult.ParsingSuccessful)
                throw new InvalidOperationException();
            return value;
        }

        [Benchmark]
        public long LightJsonUtf16()
        {
            if (SerializedValue.NumberInUtf16.AsSpan().TryParseInt32(out var value, out _) != IntegerParseResult.ParsingSuccessful)
                throw new InvalidOperationException();
            return value;
        }

        public readonly struct SerializedInt32
        {
            public readonly string NumberInUtf16;
            public readonly byte[] NumberInUtf8;

            public SerializedInt32(int number)
            {
                NumberInUtf16 = number.ToString();
                NumberInUtf8 = NumberInUtf16.ToUtf8();
            }

            public SerializedInt32(string serializedValue)
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