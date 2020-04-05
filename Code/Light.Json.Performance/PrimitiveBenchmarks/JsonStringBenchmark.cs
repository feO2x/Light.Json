using System;
using System.Text;
using BenchmarkDotNet.Attributes;
using Light.Json.FrameworkExtensions;
using Newtonsoft.Json;
using Utf8JsonSerializer = Utf8Json.JsonSerializer;

namespace Light.Json.Performance.PrimitiveBenchmarks
{
    public static class JsonStringBenchmarkConstants
    {
        public const string ShortJson = "\"This is a short JSON string\"";
        public const string LongJson = "\"\\\"When the sun rises in the west and sets in the east,\\\" she said sadly. \\\"When the seas go dry and mountains blow in the wind like leaves. When my womb quickens again, and I bear a living child. Then you will return, my sun-and-stars, and not before.\\\"\"";
    }

    public class ShortUtf16JsonStringBenchmark
    {
        private JsonSerializer _jsonNetSerializer;
        private JsonDeserializer _lightJsonSerializer;
        public string ShortUtf16Json = JsonStringBenchmarkConstants.ShortJson;

        [GlobalSetup(Target = nameof(JsonNetSerializer))]
        public void GlobalJsonNetSerializerSetup() => _jsonNetSerializer = JsonSerializer.CreateDefault();

        [GlobalSetup(Target = nameof(LightJson))]
        public void GlobalLightJsonSetup() => _lightJsonSerializer = new JsonDeserializer();

        [Benchmark(Baseline = true)]
        public string JsonNetConvert()
        {
            return JsonConvert.DeserializeObject<string>(ShortUtf16Json);
        }

        [Benchmark]
        public string JsonNetSerializer()
        {
            using (var reader = JsonNet.CreateJsonNetTextReader(ShortUtf16Json))
            {
                return _jsonNetSerializer.Deserialize<string>(reader);
            }
        }

        [Benchmark]
        public string LightJson() => _lightJsonSerializer.Deserialize<string>(ShortUtf16Json.AsMemory());

        [Benchmark]
        public string Utf8Json() => Utf8JsonSerializer.Deserialize<string>(ShortUtf16Json);
    }

    public class LongUtf16JsonStringBenchmark
    {
        private JsonSerializer _jsonNetSerializer;
        private JsonDeserializer _lightJsonSerializer;
        public string LongUtf16Json = JsonStringBenchmarkConstants.LongJson;

        [GlobalSetup(Target = nameof(JsonNetSerializer))]
        public void GlobalJsonNetSerializerSetup() => _jsonNetSerializer = JsonSerializer.CreateDefault();

        [GlobalSetup(Target = nameof(LightJson))]
        public void GlobalLightJsonSetup() => _lightJsonSerializer = new JsonDeserializer();

        [Benchmark(Baseline = true)]
        public string JsonNetConvert()
        {
            return JsonConvert.DeserializeObject<string>(LongUtf16Json);
        }

        [Benchmark]
        public string JsonNetSerializer()
        {
            using (var reader = JsonNet.CreateJsonNetTextReader(LongUtf16Json))
            {
                return _jsonNetSerializer.Deserialize<string>(reader);
            }
        }

        [Benchmark]
        public string LightJson() => _lightJsonSerializer.Deserialize<string>(LongUtf16Json.AsMemory());

        [Benchmark]
        public string Utf8Json() => Utf8JsonSerializer.Deserialize<string>(LongUtf16Json);
    }

    public class ShortUtf8JsonStringBenchmark
    {
        private JsonSerializer _jsonNetSerializer;
        private JsonDeserializer _lightJsonSerializer;
        public byte[] ShortUtf8Json = JsonStringBenchmarkConstants.ShortJson.ToUtf8();

        [GlobalSetup(Target = nameof(JsonNetSerializer))]
        public void GlobalJsonNetSerializerSetup() => _jsonNetSerializer = JsonSerializer.CreateDefault();

        [GlobalSetup(Target = nameof(LightJson))]
        public void GlobalLightJsonSetup() => _lightJsonSerializer = new JsonDeserializer();

        [Benchmark(Baseline = true)]
        public string JsonNetConvert()
        {
            var utf16 = Encoding.UTF8.GetString(ShortUtf8Json);
            return JsonConvert.DeserializeObject<string>(utf16);
        }

        [Benchmark]
        public string JsonNetSerializer()
        {
            using (var reader = JsonNet.CreateJsonNetTextReader(ShortUtf8Json))
            {
                return _jsonNetSerializer.Deserialize<string>(reader);
            }
        }

        [Benchmark]
        public string LightJson() => _lightJsonSerializer.Deserialize<string>(ShortUtf8Json);

        [Benchmark]
        public string Utf8Json() => Utf8JsonSerializer.Deserialize<string>(ShortUtf8Json);
    }

    public class LongUtf8JsonStringBenchmark
    {
        private JsonSerializer _jsonNetSerializer;
        private JsonDeserializer _lightJsonSerializer;
        public byte[] LongUtf8Json = JsonStringBenchmarkConstants.LongJson.ToUtf8();

        [GlobalSetup(Target = nameof(JsonNetSerializer))]
        public void GlobalJsonNetSerializerSetup() => _jsonNetSerializer = JsonSerializer.CreateDefault();

        [GlobalSetup(Target = nameof(LightJson))]
        public void GlobalLightJsonSetup() => _lightJsonSerializer = new JsonDeserializer();

        [Benchmark(Baseline = true)]
        public string JsonNetConvert()
        {
            var utf16 = Encoding.UTF8.GetString(LongUtf8Json);
            return JsonConvert.DeserializeObject<string>(utf16);
        }

        [Benchmark]
        public string JsonNetSerializer()
        {
            using (var reader = JsonNet.CreateJsonNetTextReader(LongUtf8Json))
            {
                return _jsonNetSerializer.Deserialize<string>(reader);
            }
        }

        [Benchmark]
        public string LightJson() => _lightJsonSerializer.Deserialize<string>(LongUtf8Json);

        [Benchmark]
        public string Utf8Json() => Utf8JsonSerializer.Deserialize<string>(LongUtf8Json);
    }
}