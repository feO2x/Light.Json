using System.Text;
using Light.GuardClauses;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class CodeSink
    {
        private readonly StringBuilder _stringBuilder;

        public CodeSink(StringBuilder stringBuilder, string indentationCharacters = "    ")
        {
            _stringBuilder = stringBuilder.MustNotBeNull(nameof(stringBuilder));
            IndentationCharacters = indentationCharacters.MustNotBeNull(nameof(indentationCharacters));
        }


        public int CurrentIndentationLevel { get; private set; }

        public string IndentationCharacters { get; }

        public bool IsAtBeginningOfLine { get; private set; } = true;

        public CodeSink IncreaseIndentation()
        {
            ++CurrentIndentationLevel;
            return this;
        }

        public CodeSink DecreaseIndentation()
        {
            --CurrentIndentationLevel;
            return this;
        }

        public CodeSink Write(string value)
        {
            WriteIndentationIfNecessary();
            _stringBuilder.Append(value);
            return this;
        }

        public CodeSink WriteLine()
        {
            _stringBuilder.AppendLine();
            IsAtBeginningOfLine = true;
            return this;
        }

        public CodeSink WriteIfPresentWithSuffix(string? value, string? suffix = " ")
        {
            if (value.IsNullOrEmpty())
                return this;

            Write(value);
            if (suffix != null)
                Write(suffix);
            return this;
        }

        public CodeSink WriteIfPresentWithPrefix(string? value, string? prefix = " ")
        {
            if (value.IsNullOrEmpty())
                return this;

            if (prefix != null)
                Write(prefix);
            Write(value);
            return this;
        }

        public CodeSink WriteLine(string value)
        {
            Write(value);
            return WriteLine();
        }

        private void WriteIndentationIfNecessary()
        {
            if (!IsAtBeginningOfLine)
                return;

            _stringBuilder.Append(IndentationCharacters);
            IsAtBeginningOfLine = false;
        }

        public override string ToString() => _stringBuilder.ToString();

        public CodeSink Reset()
        {
            _stringBuilder.Clear();
            return this;
        }
    }
}