using Light.GuardClauses;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class Class : HierarchicalSyntaxNode<Class>
    {
        public Class(string name)
        {
            Name = name.MustNotBeNullOrWhiteSpace(nameof(name));
        }

        public string? Modifiers { get; set; } = "public sealed";

        public string Name { get; }

        public string? BaseList { get; set; }

        public override void WriteSyntax(CodeSink sink)
        {
            if (!Modifiers.IsNullOrEmpty())
            {
                sink.Write(Modifiers);
                sink.Write(" ");
            }

            sink.Write("class ")
                .Write(Name);

            if (!BaseList.IsNullOrEmpty())
            {
                sink.Write(" ")
                    .Write(BaseList);
            }

            sink.WriteLine();

            WriteChildNodesInNewScope(sink);
        }
    }
}