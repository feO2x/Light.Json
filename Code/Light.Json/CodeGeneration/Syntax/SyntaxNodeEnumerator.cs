using System.Collections;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.CodeGeneration.Syntax
{
    public sealed class SyntaxNodeEnumerator : IEnumerator<ISyntaxNode>, IEnumerable<ISyntaxNode>
    {
        private readonly bool _includeRootNode;
        private readonly SyntaxNode _rootNode;
        private readonly Stack<StackEntry> _stack = new Stack<StackEntry>();
        private bool _isAtEnd;

        public SyntaxNodeEnumerator(SyntaxNode rootNode, bool includeRootNode)
        {
            _rootNode = rootNode.MustNotBeNull(nameof(rootNode));
            _includeRootNode = includeRootNode;
        }

        public IEnumerator<ISyntaxNode> GetEnumerator() => this;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool MoveNext()
        {
            if (_isAtEnd)
                return false;

            if (Current == null)
            {
                // We only jump into this block at the first MoveNext call
                // Set the root node and check if it is part of the result set.
                Current = _rootNode;
                if (_includeRootNode)
                    return true;
            }

            // We only arrive here if Current is already set
            StackEntry entry;
            int nextIndex;

            // Check if the current node has children
            if (Current is IHierarchicalSyntaxNode hierarchicalNode && hierarchicalNode.ChildNodes.Count > 0)
            {
                // If yes, check if the last stack entry belongs to this node
                if (!_stack.TryPeek(out entry) || !ReferenceEquals(entry.SyntaxNode, hierarchicalNode))
                {
                    // If not, then get the first child and end this run
                    Current = hierarchicalNode.ChildNodes[0];
                    _stack.Push(new StackEntry(hierarchicalNode, 0));
                    return true;
                }

                // If it does belong to this node, then check if we can advance to the next item in this collection
                _stack.Pop();
                nextIndex = entry.CurrentChildIndex + 1;
                if (nextIndex == hierarchicalNode.ChildNodes.Count)
                    goto ContinueWithLastFromStack;

                Current = hierarchicalNode.ChildNodes[nextIndex];
                _stack.Push(new StackEntry(hierarchicalNode, nextIndex));
                return true;
            }

            // If we arrive here, then a subtree has no more nodes left.
            // We now check if there are parents that have items left.
            ContinueWithLastFromStack:
            if (_stack.Count == 0)
                goto NoMoreNodes;

            entry = _stack.Pop();
            nextIndex = entry.CurrentChildIndex + 1;
            if (nextIndex == entry.SyntaxNode.ChildNodes.Count)
                goto ContinueWithLastFromStack;

            Current = entry.SyntaxNode.ChildNodes[nextIndex];
            _stack.Push(new StackEntry(entry.SyntaxNode, nextIndex));
            return true;

            NoMoreNodes:
            _isAtEnd = true;
            Current = null;
            return false;
        }

        public void Reset()
        {
            _stack.Clear();
            Current = null;
        }

        // Current node can be null depending on the state of the enumerator
#pragma warning disable CS8766
#pragma warning disable 8613
        public ISyntaxNode? Current { get; private set; }
#pragma warning restore 8613
#pragma warning restore CS8766

        object? IEnumerator.Current => Current;

        public void Dispose() { }

        private readonly struct StackEntry
        {
            public readonly IHierarchicalSyntaxNode SyntaxNode;
            public readonly int CurrentChildIndex;

            public StackEntry(IHierarchicalSyntaxNode syntaxNode, int currentChildIndex)
            {
                SyntaxNode = syntaxNode;
                CurrentChildIndex = currentChildIndex;
            }
        }
    }
}