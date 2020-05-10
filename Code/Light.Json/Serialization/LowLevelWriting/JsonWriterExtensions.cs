namespace Light.Json.Serialization.LowLevelWriting
{
    public static class JsonWriterExtensions
    {
        public static void WriteSingleAsciiCharacter<TJsonWriter>(this ref TJsonWriter writer, char asciiCharacter)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.EnsureCapacity(1);
            writer.WriteAscii(asciiCharacter);
        }

        public static void WriteColon<TJsonWriter>(this ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.WriteSingleAsciiCharacter(':');
        }

        public static void WriteComma<TJsonWriter>(this ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.WriteSingleAsciiCharacter(',');
        }

        public static void WriteTrue<TJsonWriter>(this ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.EnsureCapacity(4);
            writer.WriteAscii('t');
            writer.WriteAscii('r');
            writer.WriteAscii('u');
            writer.WriteAscii('e');
        }

        public static void WriteFalse<TJsonWriter>(this ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.EnsureCapacity(5);
            writer.WriteAscii('f');
            writer.WriteAscii('a');
            writer.WriteAscii('l');
            writer.WriteAscii('s');
            writer.WriteAscii('e');
        }

        public static void WriteNull<TJsonWriter>(this ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.EnsureCapacity(4);
            writer.WriteAscii('n');
            writer.WriteAscii('u');
            writer.WriteAscii('l');
            writer.WriteAscii('l');
        }
    }
}