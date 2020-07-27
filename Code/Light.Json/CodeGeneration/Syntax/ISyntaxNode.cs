namespace Light.Json.CodeGeneration.Syntax
{
    public interface ISyntaxNode : ISyntaxWriter
    {
        NewLineMode SuffixNewLineMode { get; }
    }
}