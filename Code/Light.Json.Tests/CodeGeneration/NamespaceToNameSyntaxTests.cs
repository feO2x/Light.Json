using FluentAssertions;
using Light.GuardClauses;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using static Light.Json.CodeGeneration.ContractSyntaxFactory;

namespace Light.Json.Tests.CodeGeneration
{
    public static class NamespaceToNameSyntaxTests
    {
        [Fact]
        public static void SimpleNamespace()
        {
            var namespaceSyntax = NamespaceToNameSyntax("Foo");

            namespaceSyntax.MustHaveIdentifierName("Foo");
        }

        [Fact]
        public static void TwoPartsNamespace()
        {
            var namespaceSyntax = NamespaceToNameSyntax("Foo.Bar");

            var qualifiedName = namespaceSyntax.MustBeOfType<QualifiedNameSyntax>();
            qualifiedName.Left.MustHaveIdentifierName("Foo");
            qualifiedName.Right.MustHaveIdentifierName("Bar");
        }

        [Fact]
        public static void ThreePartsNamespace()
        {
            var namespaceSyntax = NamespaceToNameSyntax("Foo.Bar.Baz");

            var outerQualifiedName = namespaceSyntax.MustBeOfType<QualifiedNameSyntax>();
            var innerQualifiedName = outerQualifiedName.Left.MustBeOfType<QualifiedNameSyntax>();
            innerQualifiedName.Left.MustHaveIdentifierName("Foo");
            innerQualifiedName.Right.MustHaveIdentifierName("Bar");
            outerQualifiedName.Right.MustHaveIdentifierName("Baz");
        }

        private static void MustHaveIdentifierName(this NameSyntax nameSyntax, string expectedName) =>
            nameSyntax.MustBeOfType<IdentifierNameSyntax>().Identifier.Text.Should().Be(expectedName);
    }
}
