using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Light.GuardClauses;
using Light.Json.Buffers;
using Light.Json.Contracts;
using Light.Json.Deserialization;
using Light.Json.Deserialization.Tokenization;
using Light.Json.Deserialization.Tokenization.Utf16;
using Light.Json.Deserialization.Tokenization.Utf8;
using Light.Json.Serialization;
using Light.Json.Serialization.LowLevelWriting;

namespace Light.Json
{
    public sealed class JsonSerializer
    {
        private readonly ISerializationContractProvider _serializationContractProvider;
        private readonly IBufferProvider<char> _utf16InMemoryBufferProvider;
        private readonly IBufferProvider<byte> _utf8InMemoryBufferProvider;

        public JsonSerializer(ISerializationContractProvider serializationContractProvider,
                              IBufferProvider<char> utf16InMemoryBufferProvider,
                              IBufferProvider<byte> utf8InMemoryBufferProvider)
        {
            _utf16InMemoryBufferProvider = utf16InMemoryBufferProvider.MustNotBeNull(nameof(utf16InMemoryBufferProvider));
            _utf8InMemoryBufferProvider = utf8InMemoryBufferProvider.MustNotBeNull(nameof(utf8InMemoryBufferProvider));
            _serializationContractProvider = serializationContractProvider.MustNotBeNull(nameof(serializationContractProvider));
        }

        public static JsonSerializer CreateDefault() =>
            new JsonSerializer(new ImmutableSerializationContractProvider(new Dictionary<TypeKey, ISerializationContract>()), new ArrayPoolBufferProvider<char>(), new ArrayPoolBufferProvider<byte>());

        public string SerializeToUtf16<T>(T value, string? contractKey = null)
        {
            var writer = new JsonUtf16Writer(_utf16InMemoryBufferProvider);
            var context = new SerializationContext();
            Serialize(value, context, ref writer, contractKey);
            return writer.Finish();
        }

        public byte[] SerializeToUtf8<T>(T value, string? contractKey = null)
        {
            var writer = new JsonUtf8Writer(_utf8InMemoryBufferProvider);
            var context = new SerializationContext();
            Serialize(value, context, ref writer, contractKey);
            return writer.Finish();
        }

        public void Serialize<TValue, TJsonWriter>(TValue value, SerializationContext context, ref TJsonWriter writer, string? contractKey)
            where TJsonWriter : struct, IJsonWriter
        {
            var targetType = typeof(TValue);
            if (!_serializationContractProvider.TryGetContract(new TypeKey(targetType, contractKey), out ISerializeOnlyContract<TValue>? contract))
                throw new SerializationException($"The type \"${targetType}\" cannot be serialized because there is no contract registered with the serializer.");

            contract.Serialize(value, context, ref writer);
        }

        public T Deserialize<T>(string utf16Json, string? contractKey = null) => Deserialize<T>(utf16Json.AsMemory(), contractKey);

        public T Deserialize<T>(ReadOnlyMemory<char> utf16Json, string? contractKey = null)
        {
            var tokenizer = new JsonUtf16Tokenizer(utf16Json);
            var context = new DeserializationContext(this);
            return Deserialize<T, JsonUtf16Tokenizer, JsonUtf16Token>(ref context, ref tokenizer, contractKey);
        }

        public T Deserialize<T>(byte[] utf8Json, string? contractKey = null) => Deserialize<T>(utf8Json.AsMemory(), contractKey);

        public T Deserialize<T>(ReadOnlyMemory<byte> utf8Json, string? contractKey = null)
        {
            var tokenizer = new JsonUtf8Tokenizer(utf8Json);
            var context = new DeserializationContext(this);
            return Deserialize<T, JsonUtf8Tokenizer, JsonUtf8Token>(ref context, ref tokenizer, contractKey);
        }

        public TResult Deserialize<TResult, TJsonTokenizer, TJsonToken>(ref DeserializationContext context, ref TJsonTokenizer tokenizer, string? contractKey)
            where TJsonTokenizer : struct, IJsonTokenizer<TJsonToken>
            where TJsonToken : struct, IJsonToken
        {
            if (typeof(TResult) == typeof(string))
            {
                var @string = tokenizer.ReadString();
                return Unsafe.As<string, TResult>(ref @string);
            }

            if (typeof(TResult) == typeof(int))
            {
                var int32 = tokenizer.ReadInt32();
                return Unsafe.As<int, TResult>(ref int32);
            }

            var targetType = typeof(TResult);
            if (!_serializationContractProvider.TryGetContract(new TypeKey(targetType, contractKey), out IDeserializeOnlyContract<TResult>? contract))
                throw new SerializationException($"The type \"${targetType}\" cannot be deserialized because there is no corresponding contract registered with the serializer.");

            return contract.Deserialize<TJsonTokenizer, TJsonToken>(context, ref tokenizer);
        }
    }
}