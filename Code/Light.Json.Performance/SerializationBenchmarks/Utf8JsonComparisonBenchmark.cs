using System;
using BenchmarkDotNet.Attributes;
using Light.Json.Tests.Serialization;
using Light.Json.Tests.SerializationSubjects;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;

namespace Light.Json.Performance.SerializationBenchmarks
{
    public class Utf8JsonComparisonBenchmark
    {
        public static readonly Person Person = new Person { FirstName = "Kenny", LastName = "Pflug", Age = 33 };

        [Benchmark]
        public Memory<byte> MyImplementation()
        {
            return PerformanceSerializationTests.Serialize(Person);
        }

        [Benchmark]
        public byte[] Utf8Json()
        {
            return Utf8JsonSerializer.Serialize(Person);
        }
    }
}