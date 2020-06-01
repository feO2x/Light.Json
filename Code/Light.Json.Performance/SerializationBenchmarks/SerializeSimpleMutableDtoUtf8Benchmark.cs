using System.Collections.Generic;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using Light.Json.Buffers;
using Light.Json.Contracts;
using Light.Json.Tests.Serialization;
using Light.Json.Tests.SerializationSubjects;
using JsonNetSerializer = Newtonsoft.Json.JsonSerializer;
using LightJsonSerializer = Light.Json.JsonSerializer;
using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;

namespace Light.Json.Performance.SerializationBenchmarks
{
    public class SerializeSimpleMutableDtoUtf8Benchmark
    {
        public static readonly Person Person = new Person { FirstName = "Kenny", LastName = "Pflug", Age = 33 };
        public JsonNetSerializer JsonNetSerializer;
        public LightJsonSerializer LightJsonSerializer;

        [GlobalSetup(Target = nameof(JsonNet))]
        public void SetupJsonNet()
        {
            JsonNetSerializer = JsonNetSerializer.CreateDefault();
        }

        [GlobalSetup(Target = nameof(LightJson))]
        public void SetupLightJson()
        {
            LightJsonSerializer = new LightJsonSerializer(
                new ImmutableSerializationContractProvider(
                    new Dictionary<TypeKey, ISerializationContract>
                    {
                        [typeof(Person)] = new SerializeSimpleMutableObjectTests.PersonContract()
                    }
                ),
                new ArrayPoolBufferProvider<char>(),
                new ArrayPoolBufferProvider<byte>()
            );
        }

        [Benchmark(Baseline = true)]
        public byte[] JsonNet()
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
            JsonNetSerializer.Serialize(streamWriter, Person);
            return memoryStream.ToArray();
        }

        [Benchmark]
        public byte[] LightJson() => LightJsonSerializer.SerializeToUtf8(Person);

        [Benchmark]
        public byte[] SystemTextJson() => SystemTextJsonSerializer.SerializeToUtf8Bytes(Person);

        [Benchmark]
        public byte[] Utf8Json() => Utf8JsonSerializer.Serialize(Person);
    }
}