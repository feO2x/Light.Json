using System.Collections.Generic;

namespace Light.Json.CodeGeneration.Syntax
{
    public interface IHierarchicalSyntaxNode
    {
        IReadOnlyList<ISyntaxNode> ChildNodes { get; }
    }
}