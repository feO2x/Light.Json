using System;
using Light.Json.Buffers;
using Light.Json.Contracts;
using Light.Json.Deserialization;
using Light.Json.Deserialization.Tokenization;
using Light.Json.Serialization;
using Light.Json.Serialization.LowLevelWriting;

namespace Light.Json.Tests.SerializationSubjects
{
    public sealed class PersonContract : SerializationContract<Person>
    {
        public readonly ConstantValue Age = nameof(Person.Age);
        public readonly ConstantValue FirstName = nameof(Person.FirstName);
        public readonly ConstantValue LastName = nameof(Person.LastName);
        public readonly ConstantValue Segment1;
        public readonly ConstantValue Segment2;
        public readonly ConstantValue Segment3;

        public PersonContract()
        {
            Segment1 = ConstantValue.Unaltered("{\"" + FirstName + "\":");
            Segment2 = ConstantValue.Unaltered(",\"" + LastName + "\":");
            Segment3 = ConstantValue.Unaltered(",\"" + Age + "\":");
        }

        public override void Serialize<TJsonWriter>(Person person, SerializationContext context, ref TJsonWriter writer)
        {
            if (writer.IsCompatibleWithOptimizedContract)
            {
                SerializeFast(person, ref writer);
                return;
            }

            writer.WriteBeginOfObject();

            writer.WriteConstantValueAsObjectKey(FirstName);
            writer.WriteKeyValueSeparator();
            writer.WriteString(person.FirstName.AsSpan());
            writer.WriteValueSeparator();

            writer.WriteConstantValueAsObjectKey(LastName);
            writer.WriteKeyValueSeparator();
            writer.WriteString(person.LastName.AsSpan());
            writer.WriteValueSeparator();

            writer.WriteConstantValueAsObjectKey(Age);
            writer.WriteKeyValueSeparator();
            writer.WriteInteger(person.Age);

            writer.WriteEndOfObject();
        }

        private void SerializeFast<TJsonWriter>(Person person, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.WriteConstantValueLarge(Segment1);
            writer.WriteString(person.FirstName.AsSpan());
            writer.WriteConstantValueLarge(Segment2);
            writer.WriteString(person.LastName.AsSpan());
            writer.WriteConstantValue7(Segment3);
            writer.WriteInteger(person.Age);
            writer.WriteEndOfObject();
        }

        public override Person Deserialize<TJsonTokenizer, TJsonToken>(DeserializationContext context, ref TJsonTokenizer tokenizer)
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