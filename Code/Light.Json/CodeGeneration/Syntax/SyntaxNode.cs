﻿namespace Light.Json.CodeGeneration.Syntax
{
    public abstract class SyntaxNode : ISyntaxWriter
    {
        public NewLineMode SuffixNewLineMode { get; set; } = NewLineMode.None;

        public abstract void WriteSyntax(CodeSink sink);
    }
}