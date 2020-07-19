using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Light.Json.CodeGeneration.Syntax;
using Xunit;

namespace Light.Json.Tests.CodeGeneration.Syntax
{
    public sealed class SyntaxNodeTests
    {
        public SyntaxNodeTests()
        {
            UsingStatements = new UsingStatementsBlock().Add("Light.Json");
            Class1 = new Class("Bar");
            Namespace1 = new Namespace("Foo").AddChildNode(Class1);
            Class2 = new Class("Qux");
            Namespace2 = new Namespace("Baz").AddChildNode(Class2);
            Document = new Document().AddChildNode(UsingStatements)
                                      .AddChildNode(Namespace1)
                                      .AddChildNode(Namespace2);
        }

        private UsingStatementsBlock UsingStatements { get; }

        private Class Class1 { get; }

        private Namespace Namespace1 { get; }

        private Class Class2 { get; }

        private Namespace Namespace2 { get; }

        private Document Document { get; }

        [Fact]
        public void DescendantNodesAndSelf()
        {
            var flattenedNodes = Document.DescendantNodesAndSelf.ToList();

            var expected = new List<SyntaxNode>
            {
                Document,
                UsingStatements,
                Namespace1,
                Class1,
                Namespace2,
                Class2
            };
            flattenedNodes.Should().Equal(expected);
        }

        [Fact]
        public void DescendantNodes()
        {
            var flattenedNodes = Document.DescendantNodes.ToList();

            var expected = new List<SyntaxNode>
            {
                UsingStatements,
                Namespace1,
                Class1,
                Namespace2,
                Class2
            };
            flattenedNodes.Should().Equal(expected);
        }
    }
}