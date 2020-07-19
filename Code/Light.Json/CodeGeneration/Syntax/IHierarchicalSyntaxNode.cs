using System.Collections.Generic;

namespace Light.Json.CodeGeneration.Syntax
{
    public interface IHierarchicalSyntaxNode
    {
        List<SyntaxNode> ChildNodes { get; }
    }
}