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

        public IEnumerable<SyntaxNode> DescendantNodes =>
            new SyntaxNodeEnumerator(this, false);

        public IEnumerable<SyntaxNode> DescendantNodesAndSelf =>
            new SyntaxNodeEnumerator(this, true);

        public List<SyntaxNode> ChildNodes { get; }

        public T AddChildNode(SyntaxNode childNode)
        {
            ChildNodes.Add(childNode);
            return _this;
        }

        public bool RemoveChildNode(SyntaxNode childNode) =>
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