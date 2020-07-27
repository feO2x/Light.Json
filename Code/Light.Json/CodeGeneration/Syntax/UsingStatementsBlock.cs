using System.Collections.Generic;
using System.Linq;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class UsingStatementsBlock : SyntaxNode, IDocumentChild
    {
        public HashSet<UsingStatement> UsingStatements { get; } = new HashSet<UsingStatement>();

        public UsingStatementsSortOrder SortOrder { get; set; } = UsingStatementsSortOrder.AscendingWithSystemNamespacesInFront;

        public bool IncludeEmptyLineAfterStatements { get; set; } = true;

        public UsingStatementsBlock Add(string @namespace)
        {
            UsingStatements.Add(new UsingStatement(@namespace));
            return this;
        }

        public override void WriteSyntax(CodeSink sink)
        {
            if (UsingStatements.Count == 0)
                return;

            foreach (var usingStatement in SortStatements())
            {
                usingStatement.WriteSyntax(sink);
            }

            if (IncludeEmptyLineAfterStatements)
                sink.WriteLine();
        }

        private List<UsingStatement> SortStatements()
        {
            if (SortOrder == UsingStatementsSortOrder.Ascending)
            {
                var list = UsingStatements.ToList();
                list.Sort();
                return list;
            }

            var usingStatements = new List<UsingStatement>();
            var groups =
                UsingStatements.GroupBy(statement => statement.Namespace.StartsWith("System"))
                               .OrderByDescending(group => group.Key);
            foreach (var group in groups)
            {
                foreach (var usingStatement in group.OrderBy(statement => statement.Namespace))
                {
                    usingStatements.Add(usingStatement);
                }
            }

            return usingStatements;
        }
    }
}