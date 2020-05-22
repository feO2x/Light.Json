using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Light.Json.Contracts;
using Light.Json.Serialization.LowLevelWriting;
using Light.Json.Tests.Serialization;
using Light.Json.Tests.SerializationSubjects;
using Newtonsoft.Json;
using LightJsonSerializer = Light.Json.JsonSerializer;
using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;

namespace Light.Json.Performance.SerializationBenchmarks
{
    public class SerializeSimpleMutableDtoUtf16Benchmark
    {
        public LightJsonSerializer LightJsonSerializer;
        public static readonly Person Person = new Person { FirstName = "Kenny", LastName = "Pflug", Age = 33 };

        [GlobalSetup(Target = nameof(LightJson))]
        public void SetupLightJsonSerializer()
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
        public string JsonNet() => JsonConvert.SerializeObject(Person);

        [Benchmark]
        public string LightJson() => LightJsonSerializer.SerializeToUtf16(Person);

        [Benchmark]
        public string SystemTextJson() => SystemTextJsonSerializer.Serialize(Person);

        [Benchmark]
        public string Utf8Json() => Utf8JsonSerializer.ToJsonString(Person);
    }
}