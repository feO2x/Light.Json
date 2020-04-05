using System.Collections.Generic;
using FluentAssertions;
using Light.Json.FrameworkExtensions;
using Light.Json.Parsing;
using Light.Json.Tokenization;
using Xunit;

namespace Light.Json.Tests.Deserialization
{
    public sealed class SimpleMutableObjectTests
    {
        private const string Json = @"
{
    ""firstName"": ""Kenny"",
    ""lastName"": ""Pflug"",
    ""age"": 33
}";

        private readonly JsonDeserializer _deserializer =
            new JsonDeserializer(
                new ImmutableTypeParserProvider(
                    new Dictionary<TypeKey, ITypeParser>
                    {
                        [typeof(Person)] = new PersonParser()
                    }));

        [Fact]
        public void DeserializeSimpleObjectUtf16()
        {
            var deserializedPerson = _deserializer.Deserialize<Person>(Json);
            CheckDeserializedObject(deserializedPerson);
        }

        [Fact]
        public void DeserializeSimpleObjectUtf8()
        {
            var deserializedPerson = _deserializer.Deserialize<Person>(Json.ToUtf8());
            CheckDeserializedObject(deserializedPerson);
        }

        private static void CheckDeserializedObject(Person deserializedObject) =>
            deserializedObject.Should().BeEquivalentTo(new Person { FirstName = "Kenny", LastName = "Pflug", Age = 33 });

        public sealed class Person
        {
            public string? FirstName { get; set; }

            public string? LastName { get; set; }

            public int Age { get; set; }
        }

        public sealed class PersonParser : BaseTypeParser<Person>
        {
            public readonly DeserializationConstant FirstName = "firstName";
            public readonly DeserializationConstant LastName = "lastName";
            public readonly DeserializationConstant Age = "age";

            public override Person Parse<TJsonTokenizer, TJsonToken>(in DeserializationContext context, ref TJsonTokenizer tokenizer)
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