﻿using System;

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
                        writer.WriteCharacter(character);
                        break;
                }
            }

            writer.WriteAscii('\"');
        }
    }
}