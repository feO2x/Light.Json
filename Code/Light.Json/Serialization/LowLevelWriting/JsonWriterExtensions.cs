using System;
using System.Runtime.Serialization;

namespace Light.Json.Serialization.LowLevelWriting
{
    public static class JsonWriterExtensions
    {
        public static void WriteSingleAsciiCharacter<TJsonWriter>(this ref TJsonWriter writer, char asciiCharacter)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.EnsureCapacityFromCurrentIndex(1);
            writer.WriteAscii(asciiCharacter);
        }

        public static void WriteTrue<TJsonWriter>(this ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.EnsureCapacityFromCurrentIndex(4);
            writer.WriteAscii('t');
            writer.WriteAscii('r');
            writer.WriteAscii('u');
            writer.WriteAscii('e');
        }

        public static void WriteFalse<TJsonWriter>(this ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.EnsureCapacityFromCurrentIndex(5);
            writer.WriteAscii('f');
            writer.WriteAscii('a');
            writer.WriteAscii('l');
            writer.WriteAscii('s');
            writer.WriteAscii('e');
        }

        public static void WriteNull<TJsonWriter>(this ref TJsonWriter writer)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.EnsureCapacityFromCurrentIndex(4);
            writer.WriteAscii('n');
            writer.WriteAscii('u');
            writer.WriteAscii('l');
            writer.WriteAscii('l');
        }

        public static void WriteString<TJsonWriter>(this ref TJsonWriter writer, ReadOnlySpan<char> @string)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.EnsureCapacityFromCurrentIndex(@string.Length + 2);

            writer.WriteAscii('\"');
            for (var i = 0; i < @string.Length; i++)
            {
                var character = @string[i];
                switch (character)
                {
                    case '"':
                    case '\\':
                    case '\b':
                    case '\f':
                    case '\n':
                    case '\r':
                    case '\t':
                        writer.WriteEscapedCharacter(character);
                        break;
                    default:

                        if (char.IsHighSurrogate(character))
                        {
                            if (++i == @string.Length)
                                throw new SerializationException("The following UTF-16 string ends with a high surrogate and cannot be processed (the low surrogate is missing):" + Environment.NewLine + "\"" + @string.ToString() + "\"");

                            var lowSurrogate = @string[i];
                            if (!char.IsLowSurrogate(lowSurrogate))
                                throw new SerializationException($"In the following UTF-16 string, the high surrogate \"{(int) character:X4}\" at index {i - 1} is not followed by a low surrogate: {Environment.NewLine}{@string.ToString()}");

                            writer.WriteSurrogatePair(character, lowSurrogate);
                            break;
                        }

                        writer.WriteCharacter(character);
                        break;
                }
            }

            writer.WriteAscii('\"');
        }
    }
}