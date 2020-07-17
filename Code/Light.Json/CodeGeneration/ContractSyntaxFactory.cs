using System;
using Light.GuardClauses;
using Light.Json.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Light.Json.CodeGeneration
{
    public static class ContractSyntaxFactory
    {
        public static ClassDeclarationSyntax ContractClass(string name, Type serializationType, ContractKind contractKind) =>
            ClassDeclaration(name)
               .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.SealedKeyword)));

        public static SimpleBaseTypeSyntax ContractBaseClass(Type serializationType, ContractKind contractKind)
        {
            var baseContractClassName = DetermineBaseClassNameFromContractKind(contractKind);
            return SimpleBaseType(
                GenericName(
                        Identifier(baseContractClassName))
                   .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(
                                IdentifierName(serializationType.Name)
                            )
                        )
                    )
            );
        }

        public static string DetermineBaseClassNameFromContractKind(ContractKind contractKind) =>
            contractKind switch
            {
                ContractKind.SerializeOnly => "SerializeOnlyContract",
                ContractKind.DeserializeOnly => "DeserializeOnlyContract",
                ContractKind.Full => "SerializationContract",
                _ => throw new ArgumentOutOfRangeException(nameof(contractKind), contractKind, null)
            };

        public static SyntaxList<UsingDirectiveSyntax> BasicContractUsingDirectives(string serializationTypeNamespace, ContractKind contractKind) =>
            contractKind switch
            {
                ContractKind.SerializeOnly => new SyntaxList<UsingDirectiveSyntax>(new[]
                {
                    UsingDirective(NamespaceToNameSyntax("Light.Json.Contracts")),
                    UsingDirective(NamespaceToNameSyntax("Light.Json.Serialization")),
                    UsingDirective(NamespaceToNameSyntax("Light.Json.Serialization.LowLevelWriting")),
                    UsingDirective(NamespaceToNameSyntax(serializationTypeNamespace))
                }),
                ContractKind.DeserializeOnly => new SyntaxList<UsingDirectiveSyntax>(new[]
                {
                    UsingDirective(NamespaceToNameSyntax("Light.Json.Contracts")),
                    UsingDirective(NamespaceToNameSyntax("Light.Json.Deserialization")),
                    UsingDirective(NamespaceToNameSyntax("Light.Json.Deserialization.Tokenization")),
                    UsingDirective(NamespaceToNameSyntax(serializationTypeNamespace))
                }),
                ContractKind.Full => new SyntaxList<UsingDirectiveSyntax>(new[]
                {
                    UsingDirective(NamespaceToNameSyntax("Light.Json.Contracts")),
                    UsingDirective(NamespaceToNameSyntax("Light.Json.Serialization")),
                    UsingDirective(NamespaceToNameSyntax("Light.Json.Serialization.LowLevelWriting")),
                    UsingDirective(NamespaceToNameSyntax("Light.Json.Deserialization")),
                    UsingDirective(NamespaceToNameSyntax("Light.Json.Deserialization.Tokenization")),
                    UsingDirective(NamespaceToNameSyntax(serializationTypeNamespace))
                }),
                _ => throw new ArgumentOutOfRangeException(nameof(contractKind), contractKind, null)
            };

        public static NameSyntax NamespaceToNameSyntax(string @namespace)
        {
            @namespace.MustNotBeNullOrWhiteSpace(nameof(@namespace));

            var indexOfDot = @namespace.IndexOf('.');
            if (indexOfDot == -1)
                return IdentifierName(@namespace);

            return CreateQualifiedNamespaceName(@namespace, indexOfDot);
        }

        private static NameSyntax CreateQualifiedNamespaceName(string @namespace, int indexOfLastDot)
        {
            NameSyntax left = IdentifierName(@namespace.Substring(0, indexOfLastDot));
            while (true)
            {
                var indexOfNextDot = @namespace.IndexOf('.', indexOfLastDot + 1);
                IdentifierNameSyntax right;
                if (indexOfNextDot == -1)
                {
                    right = IdentifierName(@namespace.Substring(indexOfLastDot + 1));
                    return QualifiedName(left, right);
                }

                var rightStartIndex = indexOfLastDot + 1;
                right = IdentifierName(@namespace.Substring(rightStartIndex, indexOfNextDot - rightStartIndex));
                left = QualifiedName(left, right);
                indexOfLastDot = indexOfNextDot;
            }
        }
    }
}