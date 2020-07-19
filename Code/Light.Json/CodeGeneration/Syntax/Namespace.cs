using Light.GuardClauses;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class Namespace : HierarchicalSyntaxNode<Namespace>
    {
        public Namespace(string name) =>
            Name = name.MustNotBeNullOrWhiteSpace(nameof(name));

        public string Name { get; }

        public override void WriteSyntax(CodeSink sink)
        {
            sink.WriteLine("namespace " + Name)
                .WriteLine("{")
                .IncreaseIndentation();

            WriteChildNodes(sink);

            sink.DecreaseIndentation()
                .WriteLine("}");
        }
    }
}