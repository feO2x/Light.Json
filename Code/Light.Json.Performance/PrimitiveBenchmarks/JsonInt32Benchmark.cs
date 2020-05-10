using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;
using SystemTextJsonSerializer = System.Text.Json.JsonSerializer;

namespace Light.Json.Performance.PrimitiveBenchmarks
{
    public class DeserializeInt32Utf16Benchmark
    {
        private Newtonsoft.Json.JsonSerializer _jsonNetSerializer;
        private JsonSerializer _lightJsonSerializer;

        [ParamsSource(nameof(NumbersInJson))] public string Json;

        public static string[] NumbersInJson { get; } =
        {
            "42",
            "-3992",
            "2147483647",
            "-2147483647"
        };

        [GlobalSetup(Target = nameof(JsonNetSerializer))]
        public void GlobalJsonNetSerializerSetup() => _jsonNetSerializer = Newtonsoft.Json.JsonSerializer.CreateDefault();

        [GlobalSetup(Target = nameof(LightJson))]
        public void GlobalLightJsonSetup() => _lightJsonSerializer = new JsonSerializer();

        [Benchmark(Baseline = true)]
        public int SystemTextJson() => SystemTextJsonSerializer.Deserialize<int>(Json);

        [Benchmark]
        public int JsonNetConvert() => JsonConvert.DeserializeObject<int>(Json);

        [Benchmark]
        public int JsonNetSerializer()
        {
            using (var reader = JsonNet.CreateJsonNetTextReader(Json))
            {
                return _jsonNetSerializer.Deserialize<int>(reader);
            }
        }

        [Benchmark]
        public int LightJson() => _lightJsonSerializer.Deserialize<int>(Json.AsMemory());

        [Benchmark]
        public int Utf8Json() => Utf8JsonSerializer.Deserialize<int>(Json);
    }

    public class DeserializeInt32Utf8Benchmark
    {
        [ParamsSource(nameof(NumbersInJson))] public static Utf8JsonSource JsonSource;

        private Newtonsoft.Json.JsonSerializer _jsonNetSerializer;
        private JsonSerializer _lightJsonSerializer;

        public static Utf8JsonSource[] NumbersInJson { get; } =
        {
            "42",
            "-3992",
            "2147483647",
            "-2147483647"
        };

        [GlobalSetup(Target = nameof(JsonNetSerializer))]
        public void GlobalJsonNetSerializerSetup() => _jsonNetSerializer = Newtonsoft.Json.JsonSerializer.CreateDefault();

        [GlobalSetup(Target = nameof(LightJson))]
        public void GlobalLightJsonSetup() => _lightJsonSerializer = new JsonSerializer();

        [Benchmark(Baseline = true)]
        public int SystemTextJson() => SystemTextJsonSerializer.Deserialize<int>(JsonSource.Utf8Json);

        [Benchmark]
        public int JsonNetConvert() => JsonConvert.DeserializeObject<int>(Encoding.UTF8.GetString(JsonSource.Utf8Json));

        [Benchmark]
        public int JsonNetSerializer()
        {
            using (var reader = JsonNet.CreateJsonNetTextReader(JsonSource.Utf8Json))
            {
                return _jsonNetSerializer.Deserialize<int>(reader);
            }
        }

        [Benchmark]
        public int LightJson() => _lightJsonSerializer.Deserialize<int>(JsonSource.Utf8Json.AsMemory());

        [Benchmark]
        public int Utf8Json() => Utf8JsonSerializer.Deserialize<int>(JsonSource.Utf8Json);
    }
}