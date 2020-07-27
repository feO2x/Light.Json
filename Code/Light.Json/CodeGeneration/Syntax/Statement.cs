using Light.GuardClauses;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class Statement : SyntaxNode, IStatement
    {
        public Statement(string expression)
        {
            Expression = expression.MustNotBeNullOrWhiteSpace(nameof(expression));
        }

        public string Expression { get; }

        public override void WriteSyntax(CodeSink sink)
        {
            sink.Write(Expression)
                .WriteLine(";");
        }
    }
}