using Light.GuardClauses;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class Field : SyntaxNode
    {
        public Field(string type, string name)
        {
            Type = type.MustNotBeNullOrWhiteSpace(nameof(type));
            Name = name.MustNotBeNullOrWhiteSpace(nameof(name));
        }

        public string? Modifiers { get; set; } = "public readonly";

        public string Type { get; }

        public string Name { get; }

        public string? InitializationExpression { get; set; }

        public override void WriteSyntax(CodeSink sink)
        {
            if (!Modifiers.IsNullOrWhiteSpace())
            {
                sink.Write(Modifiers)
                    .Write(" ");
            }

            sink.Write(Type)
                .Write(" ")
                .Write(Name);

            if (!InitializationExpression.IsNullOrWhiteSpace())
            {
                sink.Write(" = ")
                    .Write(InitializationExpression);
            }

            sink.WriteLine(";");
        }
    }
}