using System.Collections.Generic;

namespace Light.Json.CodeGeneration.Syntax
{
    public abstract class HierarchicalSyntaxNode<T, TChild> : SyntaxNode, IHierarchicalSyntaxNode
        where T : HierarchicalSyntaxNode<T, TChild>
        where TChild : class, ISyntaxNode
    {
        private readonly T _this;

        protected HierarchicalSyntaxNode(List<TChild>? childNodes = null)
        {
            ChildNodes = childNodes ?? new List<TChild>();
            _this = (T) this;
        }

        public IEnumerable<ISyntaxNode> DescendantNodes =>
            new SyntaxNodeEnumerator(this, false);

        public IEnumerable<ISyntaxNode> DescendantNodesAndSelf =>
            new SyntaxNodeEnumerator(this, true);

        public List<TChild> ChildNodes { get; }

        IReadOnlyList<ISyntaxNode> IHierarchicalSyntaxNode.ChildNodes => ChildNodes;

        public T AddChildNode(TChild childNode)
        {
            ChildNodes.Add(childNode);
            return _this;
        }

        public bool RemoveChildNode(TChild childNode) =>
            ChildNodes.Remove(childNode);

        protected void WriteChildNodes(CodeSink sink)
        {
            for (var i = 0; i < ChildNodes.Count; i++)
            {
                var childNode = ChildNodes[i];
                childNode.WriteSyntax(sink);

                switch (childNode.SuffixNewLineMode)
                {
                    case NewLineMode.NewLineIfNotLastNode:
                        if (i < ChildNodes.Count - 1)
                            sink.WriteLine();
                        break;
                    case NewLineMode.AlwaysNewLine:
                        sink.WriteLine();
                        break;
                }
            }
        }

        protected void WriteChildNodesInNewScope(CodeSink sink)
        {
            sink.WriteLine("{")
                .IncreaseIndentation();

            WriteChildNodes(sink);

            sink.DecreaseIndentation()
                .WriteLine("}");
        }
    }
}