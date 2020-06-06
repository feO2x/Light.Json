using System;
using System.Text;
using Light.Json.Tests.SerializationSubjects;

namespace Light.Json.Tests.Serialization.JsonWriterPerformance
{
    public sealed class PersonContract
    {
        public static readonly byte[] Segment1 = Encoding.UTF8.GetBytes("{\"firstName\":");
        public static readonly byte[] Segment2 = Encoding.UTF8.GetBytes(",\"lastName\":");
        public static readonly byte[] Segment3 = Encoding.UTF8.GetBytes(",\"age\":");

        public void Serialize<TJsonWriter>(Person person, ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.WriteRaw(Segment1);
            writer.WriteString(person.FirstName.AsSpan());
            writer.WriteRaw(Segment2);
            writer.WriteString(person.LastName.AsSpan());
            writer.WriteRaw(Segment3);
            writer.WriteNumber(person.Age);
            writer.WriteEndOfObject();
        }
    }
}