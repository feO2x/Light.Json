using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Light.Json.Contracts;
using Light.Json.Tests.Deserialization;
using Light.Json.Tests.SerializationSubjects;
using Newtonsoft.Json;
using LightJsonSerializer = Light.Json.JsonSerializer;
using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;

namespace Light.Json.Performance.SerializationBenchmarks
{
    public class SerializeSimpleMutableDtoUtf16Benchmark
    {
        public static readonly Person Person = new Person { FirstName = "Kenny", LastName = "Pflug", Age = 33 };
        public LightJsonSerializer LightJsonSerializer;

        [GlobalSetup(Target = nameof(LightJson))]
        public void SetupLightJsonSerializer()
        {
            LightJsonSerializer = new LightJsonSerializer(
                new JsonSerializerSettings
                {
                    ContractProvider = new ImmutableContractProvider(
                        new Dictionary<TypeKey, ISerializationContract> { [typeof(Person)] = new DeserializeSimpleMutableObjectTests.PersonContract() }
                    )
                }
            );
        }

        [Benchmark(Baseline = true)]
        public string JsonNet() => JsonConvert.SerializeObject(Person);

        [Benchmark]
        public Memory<char> LightJson()
        {
            var result = LightJsonSerializer.SerializeToUtf16(Person);
            result.Dispose();
            return result.Json;
        }

        [Benchmark]
        public string SystemTextJson() => SystemTextJsonSerializer.Serialize(Person);

        [Benchmark]
        public string Utf8Json() => Utf8JsonSerializer.ToJsonString(Person);
    }
}