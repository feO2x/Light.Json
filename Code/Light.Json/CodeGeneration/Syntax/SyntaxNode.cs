namespace Light.Json.CodeGeneration.Syntax
{
    public abstract class SyntaxNode : ISyntaxWriter
    {
        public abstract void WriteSyntax(CodeSink sink);
    }
}