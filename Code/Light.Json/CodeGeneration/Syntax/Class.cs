using Light.GuardClauses;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class Class : HierarchicalSyntaxNode<Class, IClassMember>, INamespaceChild
    {
        public Class(string name)
        {
            Name = name.MustNotBeNullOrWhiteSpace(nameof(name));
            SuffixNewLineMode = NewLineMode.NewLineIfNotLastNode;
        }

        public string? Modifiers { get; set; } = "public sealed";

        public string Name { get; }

        public string? BaseList { get; set; }

        public override void WriteSyntax(CodeSink sink)
        {
            sink.WriteIfPresentWithSuffix(Modifiers)
                .Write("class ")
                .Write(Name)
                .WriteIfPresentWithPrefix(BaseList)
                .WriteLine();

            WriteChildNodesInNewScope(sink);
        }
    }
}