using System;
using System.Runtime.Serialization;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Deserialization.Tokenization.Utf8
{
    public partial struct JsonUtf8Tokenizer : IJsonTokenizer<JsonUtf8Token>
    {
        private static readonly byte[] NewLineCharacters = Environment.NewLine.ToUtf8();
        private static readonly byte NewLineFirstCharacter;
        private readonly ReadOnlyMemory<byte> _json;

        static JsonUtf8Tokenizer()
        {
            Check.InvalidState(NewLineCharacters.Length < 1 || NewLineCharacters.Length > 2, @"The newline characters must be either 1 byte (\n) or 2 bytes long (\r\n).");
            NewLineFirstCharacter = NewLineCharacters[0];
        }

        public JsonUtf8Tokenizer(ReadOnlyMemory<byte> json)
        {
            _json = json;
            CurrentIndex = 0;
            CurrentLine = 1;
            CurrentPosition = 1;
        }

        public int CurrentIndex { get; private set; }

        public int CurrentLine { get; private set; }

        public int CurrentPosition { get; private set; }

        public JsonUtf8Token GetNextToken()
        {
            var json = _json.Span;
            if (!TryReadNextByte(json, out var currentCharacter))
                return new JsonUtf8Token(JsonTokenType.EndOfDocument, default, CurrentLine, CurrentPosition);

            switch (currentCharacter)
            {
                case (byte) JsonSymbols.QuotationMark:
                    return ReadStringToken();
                case (byte) JsonSymbols.FalseFirstCharacter:
                    return ReadConstant(JsonTokenType.False, Utf8Symbols.False);
                case (byte) JsonSymbols.TrueFirstCharacter:
                    return ReadConstant(JsonTokenType.True, Utf8Symbols.True);
                case (byte) JsonSymbols.NullFirstCharacter:
                    return ReadConstant(JsonTokenType.Null, Utf8Symbols.Null);
                case (byte) JsonSymbols.MinusSign:
                    return ReadNegativeNumber(json);
                case (byte) JsonSymbols.BeginOfObject:
                    return ReadSingleCharacter(JsonTokenType.BeginOfObject);
                case (byte) JsonSymbols.EndOfObject:
                    return ReadSingleCharacter(JsonTokenType.EndOfObject);
                case (byte) JsonSymbols.BeginOfArray:
                    return ReadSingleCharacter(JsonTokenType.BeginOfArray);
                case (byte) JsonSymbols.EndOfArray:
                    return ReadSingleCharacter(JsonTokenType.EndOfArray);
                case (byte) JsonSymbols.EntrySeparator:
                    return ReadSingleCharacter(JsonTokenType.EntrySeparator);
                case (byte) JsonSymbols.NameValueSeparator:
                    return ReadSingleCharacter(JsonTokenType.NameValueSeparator);
            }

            if (currentCharacter.IsJsonDigit())
                return ReadNumber(json);

            throw new SerializationException($"Unexpected character \"{(char) currentCharacter}\" at line {CurrentLine} position {CurrentPosition}.");
        }

        private JsonUtf8Token ReadNumber(ReadOnlySpan<byte> json)
        {
            int i;
            for (i = CurrentIndex + 1; i < json.Length; ++i)
            {
                var currentCharacter = json[i];
                if (currentCharacter.IsJsonDigit())
                    continue;

                if (currentCharacter == JsonSymbols.DecimalSymbol)
                    return ReadFloatingPointNumber(json, i);
                break;
            }

            var token = new JsonUtf8Token(JsonTokenType.IntegerNumber, _json.Slice(CurrentIndex, i - CurrentIndex), CurrentLine, CurrentPosition);
            CurrentIndex = i;
            CurrentPosition += token.Memory.Length;
            return token;
        }

        private JsonUtf8Token ReadNegativeNumber(ReadOnlySpan<byte> json)
        {
            int i;
            for (i = CurrentIndex + 1; i < json.Length; ++i)
            {
                var currentCharacter = json[i];
                if (currentCharacter.IsJsonDigit())
                    continue;

                if (currentCharacter == JsonSymbols.DecimalSymbol)
                    return ReadFloatingPointNumber(json, i);
                break;
            }

            if (i == CurrentIndex + 1)
                Throw($"Expected digit after minus sign at line {CurrentLine} position {CurrentPosition}.");

            var token = new JsonUtf8Token(JsonTokenType.IntegerNumber, _json.Slice(CurrentIndex, i - CurrentIndex), CurrentLine, CurrentPosition);
            CurrentIndex = i;
            CurrentPosition += token.Memory.Length;
            return token;
        }

        private JsonUtf8Token ReadFloatingPointNumber(ReadOnlySpan<byte> json, int decimalSymbolIndex)
        {
            int i;
            for (i = decimalSymbolIndex + 1; i < json.Length; ++i)
            {
                if (!json[i].IsJsonDigit())
                    break;
            }

            if (i == decimalSymbolIndex + 1)
            {
                var erroneousToken = GetErroneousToken();
                Throw($"Expected digit after decimal symbol in \"{erroneousToken}\" at line {CurrentLine} position {CurrentPosition}.");
            }

            var slicedSpan = _json.Slice(CurrentIndex, i - CurrentIndex);
            var token = new JsonUtf8Token(JsonTokenType.FloatingPointNumber, slicedSpan, CurrentLine, CurrentPosition);
            CurrentIndex = i;
            CurrentPosition += slicedSpan.Length;
            return token;
        }

        private JsonUtf8Token ReadConstant(JsonTokenType type, Utf8Constant expectedSymbol)
        {
            if (CurrentIndex + expectedSymbol.ByteLength > _json.Length)
                ThrowInvalidConstant(expectedSymbol);
            var slicedBytes = _json.Slice(CurrentIndex, expectedSymbol.ByteLength);
            if (!slicedBytes.Span.SequenceEqual(expectedSymbol.ByteSequence.Span))
                ThrowInvalidConstant(expectedSymbol);

            var token = new JsonUtf8Token(type, slicedBytes, CurrentLine, CurrentPosition);
            CurrentIndex += expectedSymbol.ByteLength;
            CurrentPosition += expectedSymbol.NumberOfCharacters;
            return token;
        }

        private JsonUtf8Token ReadSingleCharacter(JsonTokenType tokenType)
        {
            var memory = _json.Slice(CurrentIndex++, 1);
            var token = new JsonUtf8Token(tokenType, memory, CurrentLine, CurrentPosition++);
            return token;
        }

        private bool TrySkipWhiteSpace(ReadOnlySpan<byte> json)
        {
            for (var i = CurrentIndex; i < json.Length; ++i)
            {
                var currentByte = json[i];
                switch (currentByte)
                {
                    case (byte) JsonSymbols.Space:
                    case (byte) JsonSymbols.HorizontalTab:
                    case (byte) JsonSymbols.CarriageReturn:
                        ++CurrentIndex;
                        ++CurrentPosition;
                        continue;
                    case (byte) JsonSymbols.LineFeed:
                        ++CurrentIndex;
                        CurrentPosition = 1;
                        ++CurrentLine;
                        continue;
                    default:
                        return true;
                }
            }

            CurrentIndex = json.Length;
            return false;
        }

        private bool TryReadNextByte(ReadOnlySpan<byte> json, out byte currentCharacter)
        {
            // In this method, we search for the first character of the next token.
            var isInSingleLineComment = false;
            while (CurrentIndex < json.Length)
            {
                currentCharacter = json[CurrentIndex];

                // If the current character is not white space and we are not in a single line comment, then return it
                if (!currentCharacter.IsWhiteSpace() && !isInSingleLineComment)
                {
                    // Check if the character is the beginning of a single line comment.
                    // If not, it can be returned and processed.
                    if (currentCharacter != JsonSymbols.SingleLineCommentCharacter)
                        return true;

                    // If it is, then check if there is enough space for another slash.
                    // If not, then return the current character, as this will result
                    // in an exception reporting an unexpected character.
                    if (CurrentIndex + 1 >= _json.Length)
                        return true;

                    // Else check if the next character is actually the second slash of a comment.
                    // If it is not, then return the slash as this will result in an exception
                    // reporting an unexpected character (as above).
                    currentCharacter = json[CurrentIndex + 1];
                    if (currentCharacter != JsonSymbols.SingleLineCommentCharacter)
                    {
                        currentCharacter = (byte) '/';
                        return true;
                    }

                    // Else we are in a single line comment until we find a new line
                    isInSingleLineComment = true;
                    CurrentIndex += 2;
                    CurrentPosition += 2;
                    continue;
                }

                // If the current Character is not a new line character, advance position and index
                if (currentCharacter != NewLineFirstCharacter)
                {
                    ++CurrentIndex;
                    ++CurrentPosition;
                    continue;
                }

                // Handle new line characters with length 1
                if (NewLineCharacters.Length == 1)
                {
                    ++CurrentIndex;
                    ++CurrentLine;
                    CurrentPosition = 1;
                    isInSingleLineComment = false;
                    continue;
                }

                // Check if there is enough space for a second line character
                if (CurrentIndex + 2 >= _json.Length)
                {
                    currentCharacter = default;
                    return false;
                }

                currentCharacter = json[++CurrentIndex];
                ++CurrentPosition;
                if (currentCharacter == NewLineCharacters[1])
                {
                    ++CurrentIndex;
                    ++CurrentLine;
                    CurrentPosition = 1;
                    isInSingleLineComment = false;
                }
            }

            currentCharacter = default;
            return false;
        }

        private static void Throw(string message) =>
            throw new SerializationException(message);

        private void ThrowInvalidConstant(in Utf8Constant expectedToken)
        {
            var invalidToken = GetErroneousToken();
            if (invalidToken == null)
                throw new SerializationException($"Expected token \"{expectedToken.Utf16Text}\" at line {CurrentLine} position {CurrentPosition}.");

            throw new SerializationException($"Expected token \"{expectedToken.Utf16Text}\" but actually found \"{invalidToken}\" at line {CurrentLine} position {CurrentPosition}.");
        }

        private string GetErroneousToken()
        {
            var remainingJsonLength = _json.Length - CurrentIndex;
            var json = _json.Slice(CurrentIndex, Math.Min(remainingJsonLength, 40)).Span;

            if (json.IsEmpty)
                return "";

            if (json[0] == '"')
                return GetErroneousJsonStringToken(json);

            for (var i = 0; i < json.Length; ++i)
            {
                switch (json[i])
                {
                    case (byte) '\n':
                        var length = i;
                        if (i - 1 > 0 && json[i - 1] == '\r')
                            --length;
                        return json.ConvertFromUtf8ToString(length);
                    case (byte) ',':
                    case (byte) ':':
                    case (byte) ']':
                    case (byte) '}':
                        return json.ConvertFromUtf8ToString(i);
                }
            }

            return json.ConvertFromUtf8ToString();
        }

        private static string GetErroneousJsonStringToken(in ReadOnlySpan<byte> utf8Json)
        {
            byte previousCharacter = 0;
            for (var i = 1; i < utf8Json.Length; ++i)
            {
                var currentCharacter = utf8Json[i];
                if (currentCharacter == '"' && previousCharacter != '\\')
                    return utf8Json.ConvertFromUtf8ToString(Math.Min(utf8Json.Length, i + 1));

                previousCharacter = currentCharacter;
            }

            return utf8Json.ConvertFromUtf8ToString();
        }

        public override string ToString() =>
            $"JsonUtf8Tokenizer at Line {CurrentLine} Position {CurrentPosition}";
    }
}