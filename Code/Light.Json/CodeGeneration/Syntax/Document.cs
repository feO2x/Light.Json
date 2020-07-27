using Light.GuardClauses.Exceptions;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class Document : HierarchicalSyntaxNode<Document, IDocumentChild>
    {
        public override void WriteSyntax(CodeSink sink) => WriteChildNodes(sink);

        public Document AddUsingStatement(string @namespace)
        {
            if (!ChildNodes.TryGetFirst<UsingStatementsBlock>(out var usingStatements))
                Throw.InvalidOperation("You cannot add a namespace when this document has no UsingStatementsBlock node.");

            usingStatements.Add(@namespace);
            return this;
        }
    }
}