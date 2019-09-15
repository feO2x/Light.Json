using System;
using System.Collections.Generic;
using System.Text;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Parsing
{
    public sealed class CSharpCodeWriter
    {
        private readonly StringBuilder _codeBuilder;
        private readonly HashSet<string> _usingStatements = new HashSet<string>();
        private int _currentIndentationLevel;
        private int _numberOfIndentCharactersPerIndentationLevel;

        public CSharpCodeWriter(StringBuilder codeBuilder = null)
        {
            _codeBuilder = codeBuilder ?? new StringBuilder();
        }

        public char IndentCharacter { get; set; }

        public int NumberOfIndentCharactersPerIndentationLevel
        {
            get => _numberOfIndentCharactersPerIndentationLevel;
            set => _numberOfIndentCharactersPerIndentationLevel = value.MustBeGreaterThanOrEqualTo(0);
        }

        public int CurrentIndentationLevel
        {
            get => _currentIndentationLevel;
            set => _currentIndentationLevel = value.MustBeGreaterThanOrEqualTo(0, nameof(CurrentIndentationLevel));
        }

        public CSharpCodeWriter Write(string code = null)
        {
            _codeBuilder.Append(code);
            return this;
        }

        public CSharpCodeWriter WriteLine(string code = null)
        {
            _codeBuilder.AppendLine(code);
            return this;
        }

        public CSharpCodeWriter WriteLineAndIndent(string code = null)
        {
            WriteLine(code);
            WriteIndent();
            return this;
        }

        public CSharpCodeWriter WriteLineAndIncreaseIndent(string code = null)
        {
            WriteLine(code);
            ++_currentIndentationLevel;
            WriteIndent();
            return this;
        }

        public CSharpCodeWriter WriteLineAndDecreaseIndent(string code = null)
        {
            WriteLine(code);
            --CurrentIndentationLevel;
            WriteIndent();
            return this;
        }

        public CSharpCodeWriter WriteIndent()
        {
            _codeBuilder.Append(IndentCharacter, _currentIndentationLevel);
            return this;
        }

        public CSharpCodeWriter AddUsingStatementForNamespaceIfNecessary(Type targetType)
        {
            targetType.MustNotBeNull(nameof(targetType));

            var usingStatement = "using " + targetType.Namespace + ";";
            _usingStatements.Add(usingStatement);
            return this;
        }

        public CSharpCodeWriter ApplyUsingStatementsAtTopOfFile()
        {
            var stringBuilder = new StringBuilder();

            var sortedUsingStatements = _usingStatements.ToSortedArray();
            for (var i = 0; i < sortedUsingStatements.Length; ++i)
            {
                stringBuilder.AppendLine(sortedUsingStatements[i]);
            }

            stringBuilder.AppendLine();
            _codeBuilder.Insert(0, stringBuilder.ToString());
            return this;
        }

        public string CreateDocument() => _codeBuilder.ToString();
    }
}