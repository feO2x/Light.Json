using System.Collections.Generic;
using System.IO;
using System.Text;
using BenchmarkDotNet.Attributes;
using Light.Json.Contracts;
using Light.Json.FrameworkExtensions;
using Light.Json.Tests.SerializationSubjects;
using Newtonsoft.Json;
using JsonNetSerializer = Newtonsoft.Json.JsonSerializer;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;
using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;

namespace Light.Json.Performance.DeserializationBenchmarks
{
    public class DeserializeSimpleMutableDtoUtf8Benchmark
    {
        public byte[] Json = "{\"firstName\":\"Kenny\",\"lastName\":\"Pflug\",\"age\":33}".ToUtf8();
        public JsonNetSerializer JsonNetSerializer;
        public JsonSerializer LightJsonSerializer;

        [GlobalSetup(Target = nameof(JsonNet))]
        public void SetupJsonNet()
        {
            JsonNetSerializer = JsonNetSerializer.CreateDefault();
        }

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
        public Person JsonNet()
        {
            using var memoryStream = new MemoryStream(Json);
            using var streamReader = new StreamReader(memoryStream, Encoding.UTF8);
            using var jsonReader = new JsonTextReader(streamReader);
            return JsonNetSerializer.Deserialize<Person>(jsonReader);
        }

        [Benchmark]
        public Person SystemTextJson() => SystemTextJsonSerializer.Deserialize<Person>(Json);

        [Benchmark]
        public Person Utf8Json() => Utf8JsonSerializer.Deserialize<Person>(Json);

        [Benchmark]
        public Person LightJson() => LightJsonSerializer.Deserialize<Person>(Json);
    }
}