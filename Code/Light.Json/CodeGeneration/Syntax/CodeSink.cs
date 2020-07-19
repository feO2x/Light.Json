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
    }
}