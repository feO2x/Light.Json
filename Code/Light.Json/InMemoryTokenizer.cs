using System;

namespace Light.Json
{
    public ref struct InMemoryTokenizer
    {
        private readonly ReadOnlySpan<char> _json;
        private int _currentIndex;

        public InMemoryTokenizer(ReadOnlySpan<char> json)
        {
            _json = json;
            _currentIndex = 0;
        }

        public JsonToken GetNextToken()
        {
            if (_json[_currentIndex] == JsonTokenizerSymbols.StringDelimiter)
                return ReadJsonString();

            throw new NotImplementedException();
        }

        private JsonToken ReadJsonString()
        {
            var leftBoundedJson = _json.Slice(_currentIndex);

            for (var i = 1; i < leftBoundedJson.Length; ++i)
            {
                if (leftBoundedJson[i] != JsonTokenizerSymbols.StringDelimiter)
                    continue;

                var targetSpan = leftBoundedJson.Slice(0, i + 1);
                _currentIndex += targetSpan.Length;
                return JsonToken.String(targetSpan);
            }

            throw new DeserializationException($"Could not find end of JSON string {leftBoundedJson.ToString()}.");
        }
    }
}
