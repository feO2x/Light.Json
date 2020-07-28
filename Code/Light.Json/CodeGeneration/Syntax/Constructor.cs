using Light.GuardClauses;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class Constructor : HierarchicalSyntaxNode<Constructor, IStatement>, IClassMember
    {
        public Constructor(string typeName)
        {
            TypeName = typeName.MustNotBeNullOrWhiteSpace(nameof(typeName));
            SuffixNewLineMode = NewLineMode.NewLineIfNotLastNode;
        }

        public string? Modifiers { get; set; } = Syntax.Modifiers.Public;

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