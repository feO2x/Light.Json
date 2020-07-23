using System.Collections.Generic;
using FluentAssertions;
using Light.Json.Contracts;
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
                new JsonSerializerSettings
                {
                    ContractProvider =
                        new ImmutableContractProvider(
                            new Dictionary<TypeKey, ISerializationContract> { [typeof(Person)] = new PersonContract() }
                        )
                }
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
    }
}