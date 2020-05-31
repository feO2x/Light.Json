using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Light.Json.Buffers;
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
        public ImmutableSerializationContractProvider LightJsonAsyncContracts;
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

        [GlobalSetup(Target = nameof(LightJsonAsync))]
        public void SetupLightJsonAsync()
        {
            LightJsonAsyncContracts = new ImmutableSerializationContractProvider(
                new Dictionary<TypeKey, ISerializationContract>
                {
                    [typeof(Person)] = new SerializeSimpleMutableObjectTests.PersonContract()
                }
            );
        }

        [Benchmark(Baseline = true)]
        public string JsonNet() => JsonConvert.SerializeObject(Person);

        [Benchmark]
        public string LightJson() => LightJsonSerializer.SerializeToUtf16(Person);

        [Benchmark]
        public string LightJsonAsync() => RunLightJsonAsync().Result;

        private async Task<string> RunLightJsonAsync()
        {
            if (!LightJsonAsyncContracts.TryGetContract<SerializeSimpleMutableObjectTests.PersonContract>(typeof(Person), out var contract))
                throw new SerializationException("Could not find Person contract.");

            var jsonWriter = new AsyncJsonWriter<Utf16BufferWriter>(new Utf16BufferWriter(new InMemoryArrayPoolBufferWriterService<char>()));

            await SerializeSimpleMutableObjectTests.SerializePerson(jsonWriter, contract);

            return jsonWriter.BufferWriter.Finish();
        }

        [Benchmark]
        public string SystemTextJson() => SystemTextJsonSerializer.Serialize(Person);

        [Benchmark]
        public string Utf8Json() => Utf8JsonSerializer.ToJsonString(Person);
    }
}