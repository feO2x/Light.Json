using System;
using System.Runtime.CompilerServices;
using Light.GuardClauses;
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
        public JsonSerializer()
        {
            Settings = new JsonSerializerSettings();
        }

        public JsonSerializer(JsonSerializerSettings settings)
        {
            Settings = settings.MustNotBeNull(nameof(settings));
        }

        public JsonSerializerSettings Settings { get; }

        public Utf16SerializationResult SerializeToUtf16<T>(T value, string? contractKey = null)
        {
            var writer = new JsonUtf16Writer(Settings.Utf16BufferProvider);
            var context = new SerializationContext();
            Serialize(value, context, ref writer, contractKey);
            return writer.GetResult();
        }

        public Utf8SerializationResult SerializeToUtf8<T>(T value, string? contractKey = null)
        {
            var writer = new JsonUtf8Writer(Settings.Utf8BufferProvider);
            var context = new SerializationContext();
            Serialize(value, context, ref writer, contractKey);
            return writer.GetResult();
        }

        public void Serialize<TValue, TJsonWriter>(TValue value, SerializationContext context, ref TJsonWriter writer, string? contractKey)
            where TJsonWriter : struct, IJsonWriter
        {
            var contract = Settings.ContractProvider.GetContract<TValue, ISerializeOnlyContract<TValue>>(value, contractKey);
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

            var contract = Settings.ContractProvider.GetContract<TResult, IDeserializeOnlyContract<TResult>>(contractKey);
            return contract.Deserialize<TJsonTokenizer, TJsonToken>(context, ref tokenizer);
        }
    }
}