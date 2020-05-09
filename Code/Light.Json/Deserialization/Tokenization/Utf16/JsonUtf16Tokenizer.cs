using System;
using System.Runtime.Serialization;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Deserialization.Tokenization.Utf16
{
    public partial struct JsonUtf16Tokenizer : IJsonTokenizer<JsonUtf16Token>
    {
        private static readonly string NewLineCharacters = Environment.NewLine;
        private static readonly char NewLineFirstCharacter = NewLineCharacters[0];
        private readonly ReadOnlyMemory<char> _json;

        public JsonUtf16Tokenizer(ReadOnlyMemory<char> json)
        {
            _json = json;
            CurrentIndex = 0;
            CurrentLine = 1;
            CurrentPosition = 1;
        }

        public int CurrentIndex { get; private set; }

        public int CurrentLine { get; private set; }

        public int CurrentPosition { get; private set; }

        public JsonUtf16Token GetNextToken()
        {
            var json = _json.Span;
            if (!TryReadNextCharacter(json, out var currentCharacter))
                return new JsonUtf16Token(JsonTokenType.EndOfDocument, default, CurrentLine, CurrentPosition);
            switch (currentCharacter)
            {
                case JsonSymbols.QuotationMark:
                    return ReadStringToken();
                case JsonSymbols.FalseFirstCharacter:
                    return ReadConstant(json, JsonTokenType.False, JsonSymbols.False);
                case JsonSymbols.TrueFirstCharacter:
                    return ReadConstant(json, JsonTokenType.True, JsonSymbols.True);
                case JsonSymbols.NullFirstCharacter:
                    return ReadConstant(json, JsonTokenType.Null, JsonSymbols.Null);
                case JsonSymbols.MinusSign:
                    return ReadNegativeNumber(json);
                case JsonSymbols.BeginOfObject:
                    return ReadSingleCharacter(JsonTokenType.BeginOfObject);
                case JsonSymbols.EndOfObject:
                    return ReadSingleCharacter(JsonTokenType.EndOfObject);
                case JsonSymbols.BeginOfArray:
                    return ReadSingleCharacter(JsonTokenType.BeginOfArray);
                case JsonSymbols.EndOfArray:
                    return ReadSingleCharacter(JsonTokenType.EndOfArray);
                case JsonSymbols.EntrySeparator:
                    return ReadSingleCharacter(JsonTokenType.EntrySeparator);
                case JsonSymbols.NameValueSeparator:
                    return ReadSingleCharacter(JsonTokenType.NameValueSeparator);
            }

            if (currentCharacter.IsJsonDigit())
                return ReadNumber(json);

            throw new SerializationException($"Unexpected character \"{currentCharacter}\" at line {CurrentLine} position {CurrentPosition}.");
        }

        private bool TrySkipWhiteSpace(in ReadOnlySpan<char> json)
        {
            for (var i = CurrentIndex; i < json.Length; ++i)
            {
                switch (json[i])
                {
                    case JsonSymbols.Space:
                    case JsonSymbols.CarriageReturn:
                    case JsonSymbols.HorizontalTab:
                        ++CurrentIndex;
                        ++CurrentPosition;
                        continue;
                    case JsonSymbols.LineFeed:
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

        private bool TryReadNextCharacter(ReadOnlySpan<char> json, out char currentCharacter)
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
                        currentCharacter = '/';
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

        private JsonUtf16Token ReadSingleCharacter(JsonTokenType tokenType) =>
            new JsonUtf16Token(tokenType, _json.Slice(CurrentIndex++, 1), CurrentLine, CurrentPosition++);

        private JsonUtf16Token ReadNumber(ReadOnlySpan<char> json)
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

            var token = new JsonUtf16Token(JsonTokenType.IntegerNumber, _json.Slice(CurrentIndex, i - CurrentIndex), CurrentLine, CurrentPosition);
            CurrentIndex = i;
            CurrentPosition += token.Memory.Length;
            return token;
        }

        private JsonUtf16Token ReadFloatingPointNumber(ReadOnlySpan<char> json, int decimalSymbolIndex)
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
            var token = new JsonUtf16Token(JsonTokenType.FloatingPointNumber, slicedSpan, CurrentLine, CurrentPosition);
            CurrentIndex = i;
            CurrentPosition += slicedSpan.Length;
            return token;
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
                    case '\n':
                        var length = i;
                        if (i - 1 > 0 && json[i - 1] == '\r')
                            --length;
                        return json.Slice(0, length).ToString();
                    case ',':
                    case ':':
                    case ']':
                    case '}':
                        return json.Slice(0, i).ToString();
                }
            }

            return json.ToString();
        }

        private static string GetErroneousJsonStringToken(in ReadOnlySpan<char> json)
        {
            var previousCharacter = default(char);
            for (var i = 1; i < json.Length; ++i)
            {
                var currentCharacter = json[i];
                if (currentCharacter == '"' && previousCharacter != '\\')
                    return json.Slice(0, Math.Min(json.Length, i + 1)).ToString();

                previousCharacter = currentCharacter;
            }

            return json.ToString();
        }

        private JsonUtf16Token ReadNegativeNumber(ReadOnlySpan<char> json)
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

            var token = new JsonUtf16Token(JsonTokenType.IntegerNumber, _json.Slice(CurrentIndex, i - CurrentIndex), CurrentLine, CurrentPosition);
            CurrentIndex = i;
            CurrentPosition += token.Memory.Length;
            return token;
        }

        private JsonUtf16Token ReadConstant(ReadOnlySpan<char> json, JsonTokenType type, string expectedSymbol)
        {
            if (CurrentIndex + expectedSymbol.Length > _json.Length)
                ThrowInvalidConstant(expectedSymbol, json.Slice(CurrentIndex));
            var slicedText = _json.Slice(CurrentIndex, expectedSymbol.Length);
            if (!slicedText.Span.Equals(expectedSymbol.AsSpan(), StringComparison.Ordinal))
                ThrowInvalidConstant(expectedSymbol, slicedText.Span);

            var token = new JsonUtf16Token(type, slicedText, CurrentLine, CurrentPosition);
            CurrentIndex += expectedSymbol.Length;
            CurrentPosition += expectedSymbol.Length;
            return token;
        }

        private void ThrowInvalidConstant(string expectedTokenText, ReadOnlySpan<char> actualTokenText) =>
            Throw($"Expected token \"{expectedTokenText}\" but actually found \"{actualTokenText.ToString()}\" at line {CurrentLine} position {CurrentPosition}.");

        private static void Throw(string message) =>
            throw new SerializationException(message);

        public override string ToString() =>
            $"JsonUtf16Tokenizer at Line {CurrentLine} Position {CurrentPosition}";
    }
}