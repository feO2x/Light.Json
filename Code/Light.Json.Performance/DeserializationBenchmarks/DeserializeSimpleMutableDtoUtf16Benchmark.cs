using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Light.Json.Contracts;
using Light.Json.Tests.SerializationSubjects;
using Newtonsoft.Json;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;
using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;

namespace Light.Json.Performance.DeserializationBenchmarks
{
    public class DeserializeSimpleMutableDtoUtf16Benchmark
    {
        public string Json = "{\"firstName\":\"Kenny\",\"lastName\":\"Pflug\",\"age\":33}";
        public JsonSerializer LightJsonSerializer;

        [GlobalSetup(Target = nameof(LightJson))]
        public void SetupLightJson()
        {
            LightJsonSerializer =
                new JsonSerializer(
                    new JsonSerializerSettings
                    {
                        ContractProvider =
                            new ImmutableContractProvider(
                                new Dictionary<TypeKey, ISerializationContract>
                                {
                                    [typeof(Person)] = new PersonContract()
                                })
                    });
        }

        [Benchmark(Baseline = true)]
        public Person JsonNet() => JsonConvert.DeserializeObject<Person>(Json);

        [Benchmark]
        public Person SystemTextJson() => SystemTextJsonSerializer.Deserialize<Person>(Json);

        [Benchmark]
        public Person Utf8Json() => Utf8JsonSerializer.Deserialize<Person>(Json);

        [Benchmark]
        public Person LightJson() => LightJsonSerializer.Deserialize<Person>(Json);
    }
}