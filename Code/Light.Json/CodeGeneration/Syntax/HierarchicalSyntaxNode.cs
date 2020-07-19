using System.Collections.Generic;

namespace Light.Json.CodeGeneration.Syntax
{
    public abstract class HierarchicalSyntaxNode<T> : SyntaxNode, IHierarchicalSyntaxNode
        where T : HierarchicalSyntaxNode<T>
    {
        private readonly T _this;
        protected HierarchicalSyntaxNode(List<SyntaxNode>? childNodes = null)
        {
            ChildNodes = childNodes ?? new List<SyntaxNode>();
            _this = (T) this;
        }

        public List<SyntaxNode> ChildNodes { get; }

        public IEnumerable<SyntaxNode> DescendantNodes =>
            new SyntaxNodeEnumerator(this, false);

        public IEnumerable<SyntaxNode> DescendantNodesAndSelf =>
            new SyntaxNodeEnumerator(this, true);

        public T AddChildNode(SyntaxNode childNode)
        {
            ChildNodes.Add(childNode);
            return _this;
        }

        public bool RemoveChildNode(SyntaxNode childNode) =>
            ChildNodes.Remove(childNode);

        protected virtual void WriteChildNodes(CodeSink sink)
        {
            foreach (var childNode in ChildNodes)
            {
                childNode.WriteSyntax(sink);
            }
        }
    }
}