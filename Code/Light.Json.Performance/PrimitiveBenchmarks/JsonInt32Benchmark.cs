using System;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;

namespace Light.Json.Performance.PrimitiveBenchmarks
{
    public static class Int32BenchmarkConstants
    {
        public const int TwoDigitPositiveInt32 = 42;
        public const int FourDigitNegativeInt32 = -3992;
        public const int TenDigitPositiveInt32 = 2147483647;
        public const int TenDigitNegativeInt32 = -2147483647;
    }

    public class TwoDigitPositiveInt32Utf16Benchmark
    {
        private JsonSerializer _jsonNetSerializer;
        private JsonDeserializer _lightJsonSerializer;
        public string Json = Int32BenchmarkConstants.TwoDigitPositiveInt32.ToString();

        [GlobalSetup(Target = nameof(JsonNetSerializer))]
        public void GlobalJsonNetSerializerSetup() => _jsonNetSerializer = JsonSerializer.CreateDefault();

        [GlobalSetup(Target = nameof(LightJson))]
        public void GlobalLightJsonSetup() => _lightJsonSerializer = new JsonDeserializer();

        [Benchmark(Baseline = true)]
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
        public int Utf8Json() => global::Utf8Json.JsonSerializer.Deserialize<int>(Json);

        [Benchmark]
        public int LightJson() => _lightJsonSerializer.Deserialize<int>(Json.AsMemory());
    }

    public class FourDigitNegativeInt32Utf16Benchmark
    {
        private JsonSerializer _jsonNetSerializer;
        private JsonDeserializer _lightJsonSerializer;
        public string Json = Int32BenchmarkConstants.FourDigitNegativeInt32.ToString();

        [GlobalSetup(Target = nameof(JsonNetSerializer))]
        public void GlobalJsonNetSerializerSetup() => _jsonNetSerializer = JsonSerializer.CreateDefault();

        [GlobalSetup(Target = nameof(LightJson))]
        public void GlobalLightJsonSetup() => _lightJsonSerializer = new JsonDeserializer();

        [Benchmark(Baseline = true)]
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
        public int Utf8Json() => Utf8JsonSerializer.Deserialize<int>(Json);

        [Benchmark]
        public int LightJson() => _lightJsonSerializer.Deserialize<int>(Json.AsMemory());
    }

    public class TenDigitPositiveInt32Utf16Benchmark
    {
        private JsonSerializer _jsonNetSerializer;
        private JsonDeserializer _lightJsonSerializer;
        public string Json = Int32BenchmarkConstants.TenDigitPositiveInt32.ToString();

        [GlobalSetup(Target = nameof(JsonNetSerializer))]
        public void GlobalJsonNetSerializerSetup() => _jsonNetSerializer = JsonSerializer.CreateDefault();

        [GlobalSetup(Target = nameof(LightJson))]
        public void GlobalLightJsonSetup() => _lightJsonSerializer = new JsonDeserializer();

        [Benchmark(Baseline = true)]
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
        public int Utf8Json() => global::Utf8Json.JsonSerializer.Deserialize<int>(Json);

        [Benchmark]
        public int LightJson() => _lightJsonSerializer.Deserialize<int>(Json.AsMemory());
    }

    public class TenDigitNegativeInt32Utf16Benchmark
    {
        private JsonSerializer _jsonNetSerializer;
        private JsonDeserializer _lightJsonSerializer;
        public string Json = Int32BenchmarkConstants.TenDigitNegativeInt32.ToString();

        [GlobalSetup(Target = nameof(JsonNetSerializer))]
        public void GlobalJsonNetSerializerSetup() => _jsonNetSerializer = JsonSerializer.CreateDefault();

        [GlobalSetup(Target = nameof(LightJson))]
        public void GlobalLightJsonSetup() => _lightJsonSerializer = new JsonDeserializer();

        [Benchmark(Baseline = true)]
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
        public int Utf8Json() => Utf8JsonSerializer.Deserialize<int>(Json);

        [Benchmark]
        public int LightJson() => _lightJsonSerializer.Deserialize<int>(Json.AsMemory());
    }
}