using Light.GuardClauses;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class Constructor : HierarchicalSyntaxNode<Constructor, IStatement>
    {
        public Constructor(string typeName)
        {
            TypeName = typeName.MustNotBeNullOrWhiteSpace(nameof(typeName));
            SuffixNewLineMode = NewLineMode.NewLineIfNotLastNode;
        }

        public string? Modifiers { get; set; } = "public";

        public string TypeName { get; }

        public override void WriteSyntax(CodeSink sink)
        {
            if (!Modifiers.IsNullOrWhiteSpace())
            {
                sink.Write(Modifiers)
                    .Write(" ");
            }

            sink.Write(TypeName)
                .WriteLine("()");

            WriteChildNodesInNewScope(sink);
        }
    }
}