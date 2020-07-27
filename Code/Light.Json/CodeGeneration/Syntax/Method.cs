using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class Method : HierarchicalSyntaxNode<Method, IStatement>, IClassMember
    {
        public Method(string name)
        {
            Name = name.MustNotBeNullOrWhiteSpace(nameof(name));
            SuffixNewLineMode = NewLineMode.NewLineIfNotLastNode;
        }

        public string? Modifiers { get; set; } = "public";

        public string ReturnType { get; set; } = "void";

        public string Name { get; set; }

        public string? GenericParameters { get; set; }

        public string ParameterList { get; set; } = "()";

        public List<string>? GenericConstraints { get; set; }

        public override void WriteSyntax(CodeSink sink)
        {
            sink.WriteIfPresentWithSuffix(Modifiers)
                .Write(ReturnType)
                .Write(" ")
                .Write(Name);

            if (!GenericParameters.IsNullOrWhiteSpace())
            {
                sink.Write("<")
                    .Write(GenericParameters)
                    .Write(">");
            }

            sink.WriteLine(ParameterList);
            if (!GenericConstraints.IsNullOrEmpty())
            {
                sink.IncreaseIndentation();
                foreach (var genericConstraint in GenericConstraints)
                {
                    sink.WriteLine(genericConstraint);
                }

                sink.DecreaseIndentation();
            }

            WriteChildNodesInNewScope(sink);
        }
    }
}