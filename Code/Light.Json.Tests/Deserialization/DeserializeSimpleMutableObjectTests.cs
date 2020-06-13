using System.Collections.Generic;
using FluentAssertions;
using Light.Json.Buffers;
using Light.Json.Contracts;
using Light.Json.Deserialization;
using Light.Json.Deserialization.Tokenization;
using Light.Json.FrameworkExtensions;
using Light.Json.Tests.SerializationSubjects;
using Xunit;

namespace Light.Json.Tests.Deserialization
{
    public sealed class DeserializeSimpleMutableObjectTests
    {
        private const string Json = @"
{
    ""firstName"": ""Kenny"",
    ""lastName"": ""Pflug"",
    ""age"": 33
}";

        private readonly JsonSerializer _serializer =
            new JsonSerializer(
                new ImmutableContractProvider(
                    new Dictionary<TypeKey, ISerializationContract>
                    {
                        [typeof(Person)] = new PersonContract()
                    }
                ),
                new ArrayPoolBufferProvider<char>(),
                new ArrayPoolBufferProvider<byte>()
            );

        [Fact]
        public void DeserializeSimpleObjectUtf16()
        {
            var deserializedPerson = _serializer.Deserialize<Person>(Json);
            CheckDeserializedObject(deserializedPerson);
        }

        [Fact]
        public void DeserializeSimpleObjectUtf8()
        {
            var deserializedPerson = _serializer.Deserialize<Person>(Json.ToUtf8());
            CheckDeserializedObject(deserializedPerson);
        }

        private static void CheckDeserializedObject(Person deserializedObject) =>
            deserializedObject.Should().BeEquivalentTo(new Person { FirstName = "Kenny", LastName = "Pflug", Age = 33 });

        public sealed class PersonContract : DeserializeOnlyContract<Person>
        {
            public readonly ConstantValue Age = nameof(Person.Age);
            public readonly ConstantValue FirstName = nameof(Person.FirstName);
            public readonly ConstantValue LastName = nameof(Person.LastName);

            public override Person Deserialize<TJsonTokenizer, TJsonToken>(in DeserializationContext context, ref TJsonTokenizer tokenizer)
            {
                tokenizer.ReadBeginOfObject();
                var person = new Person();
                while (true)
                {
                    var nameToken = tokenizer.ReadNameToken();
                    if (nameToken.Equals(FirstName))
                        person.FirstName = tokenizer.ReadString();
                    else if (nameToken.Equals(LastName))
                        person.LastName = tokenizer.ReadString();
                    else if (nameToken.Equals(Age))
                        person.Age = tokenizer.ReadInt32();

                    var token = tokenizer.GetNextToken();
                    if (token.Type == JsonTokenType.EndOfObject)
                        break;
                    token.MustBeOfType(JsonTokenType.EntrySeparator);
                }

                return person;
            }
        }
    }
}