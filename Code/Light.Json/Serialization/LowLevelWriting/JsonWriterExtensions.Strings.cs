using System;
using System.Runtime.Serialization;

namespace Light.Json.Serialization.LowLevelWriting
{
    public static partial class JsonWriterExtensions
    {
        public static void WriteString<TJsonWriter>(this ref TJsonWriter writer, in ReadOnlySpan<char> @string)
            where TJsonWriter : struct, IJsonWriter
        {
            writer.EnsureCapacityFromCurrentIndex(@string.Length + 2);

            writer.WriteAscii('\"');
            if (@string.IsEmpty)
            {
                writer.WriteAscii('\"');
                return;
            }

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
                        writer.EnsureAdditionalCapacity(1);
                        writer.WriteAscii('\"');
                        writer.WriteAscii(character);
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