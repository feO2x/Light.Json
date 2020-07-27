using Light.GuardClauses;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class IfStatement : HierarchicalSyntaxNode<IfStatement, IStatement>, IStatement
    {
        public IfStatement(string condition)
        {
            Condition = condition.MustNotBeNullOrWhiteSpace(nameof(condition));
            SuffixNewLineMode = NewLineMode.NewLineIfNotLastNode;
        }

        public string Condition { get; }

        public override void WriteSyntax(CodeSink sink)
        {
            sink.Write("if (")
                .Write(Condition)
                .WriteLine(")");

            WriteChildNodesInNewScope(sink);
        }
    }
}