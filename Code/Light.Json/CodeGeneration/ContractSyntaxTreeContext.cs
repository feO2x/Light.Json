using Light.GuardClauses;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Light.Json.CodeGeneration
{
    public sealed class ContractSyntaxTreeContext
    {
        public ContractSyntaxTreeContext(CSharpCompilationContext cSharpCompilationContext,
                                         NamespaceDeclarationSyntax @namespace,
                                         ClassDeclarationSyntax contractClass,
                                         SyntaxList<UsingDirectiveSyntax> usingDirectives = default)
        {
            CSharpCompilationContext = cSharpCompilationContext.MustNotBeNull(nameof(cSharpCompilationContext));
            Namespace = @namespace.MustNotBeNull(nameof(@namespace));
            ContractClass = contractClass.MustNotBeNull(nameof(contractClass));
            UsingDirectives = usingDirectives;
        }

        public CSharpCompilationContext CSharpCompilationContext { get; }

        public SyntaxList<UsingDirectiveSyntax> UsingDirectives { get; private set; }

        public NamespaceDeclarationSyntax Namespace { get; }

        public ClassDeclarationSyntax ContractClass { get; private set; }

        public ContractSyntaxTreeContext AddUsingDirective(string fullNamespace)
        {
            AddUsingDirective(UsingDirective(IdentifierName(fullNamespace)));
            return this;
        }

        public ContractSyntaxTreeContext AddUsingDirective(UsingDirectiveSyntax usingDirective)
        {
            UsingDirectives = UsingDirectives.Add(usingDirective.MustNotBeNull(nameof(usingDirective)));
            return this;
        }

        public ContractSyntaxTreeContext AddContractMember(params MemberDeclarationSyntax[] members)
        {
            ContractClass = ContractClass.AddMembers(members);
            return this;
        }

        public CSharpCompilationContext CreateSyntaxTreeAndAddToCompilationContext()
        {
            var syntaxTree =
                SyntaxTree(
                    CompilationUnit()
                       .WithUsings(UsingDirectives)
                       .WithMembers(
                            SingletonList<MemberDeclarationSyntax>(
                                Namespace
                                   .WithMembers(
                                        SingletonList<MemberDeclarationSyntax>(ContractClass)
                                    )
                            )
                        )
                );
            CSharpCompilationContext.AddSyntaxTree(syntaxTree);
            return CSharpCompilationContext;
        }
    }
}