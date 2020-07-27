using Light.GuardClauses;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class Namespace : HierarchicalSyntaxNode<Namespace, INamespaceChild>, IDocumentChild
    {
        public Namespace(string name) =>
            Name = name.MustNotBeNullOrWhiteSpace(nameof(name));

        public string Name { get; }

        public override void WriteSyntax(CodeSink sink)
        {
            sink.WriteLine("namespace " + Name);
            WriteChildNodesInNewScope(sink);
        }
    }
}