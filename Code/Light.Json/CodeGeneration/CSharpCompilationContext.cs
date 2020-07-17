using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Light.Json.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Light.Json.CodeGeneration.ContractSyntaxFactory;

namespace Light.Json.CodeGeneration
{
    public sealed class CSharpCompilationContext
    {
        private readonly Dictionary<string, MetadataReference> _metadataReferences;
        private readonly List<SyntaxTree> _syntaxTrees;

        public CSharpCompilationContext(string targetAssemblyName,
                                        string targetNamespace,
                                        OptimizationLevel optimizationLevel = OptimizationLevel.Release,
                                        Dictionary<string, MetadataReference>? assemblyMetadataReferences = null,
                                        List<SyntaxTree>? syntaxTrees = null)
        {
            TargetAssemblyName = targetAssemblyName.MustNotBeNullOrWhiteSpace(nameof(targetAssemblyName));
            TargetNamespace = targetNamespace.MustNotBeNullOrWhiteSpace(nameof(targetNamespace));
            OptimizationLevel = optimizationLevel;
            _metadataReferences = assemblyMetadataReferences ?? new Dictionary<string, MetadataReference>();
            _syntaxTrees = syntaxTrees ?? new List<SyntaxTree>();
        }

        public string TargetAssemblyName { get; }

        public string TargetNamespace { get; }

        public OptimizationLevel OptimizationLevel { get; }

        public CSharpCompilationContext AddMetadataReferencesForType(Type type)
        {
            AssemblyMetadataResolver.AddMetadataReferencesFromType(type, _metadataReferences);
            return this;
        }

        public ContractSyntaxTreeContext CreateContract(Type serializationType,
                                                        ContractKind contractKind,
                                                        string? contractClassName = null,
                                                        string? @namespace = null)
        {
            contractClassName ??= serializationType.Name + "Contract";

            return new ContractSyntaxTreeContext(
                this,
                NamespaceDeclaration(IdentifierName(@namespace ?? TargetNamespace)),
                ContractClass(contractClassName, serializationType, contractKind),
                BasicContractUsingDirectives(serializationType.Namespace!, contractKind)
            );
        }

        public CSharpCompilationContext AddSyntaxTree(SyntaxTree syntaxTree)
        {
            _syntaxTrees.Add(syntaxTree.MustNotBeNull(nameof(syntaxTree)));
            return this;
        }

        public CSharpCompilation CreateCompilation()
        {
            return CSharpCompilation.Create(
                TargetAssemblyName,
                _syntaxTrees,
                _metadataReferences.Values,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel)
            );
        }

        public static CSharpCompilationContext CreateDefault(string targetNamespace, string? assemblyName = null)
        {
            assemblyName ??= targetNamespace + ".dll";
            var metadataReferences = new Dictionary<string, MetadataReference>();
            AssemblyMetadataResolver.AddMetadataReferencesFromType(typeof(object), metadataReferences);
            AssemblyMetadataResolver.AddMetadataReferencesFromType(typeof(CSharpCompilationContext), metadataReferences);
            return new CSharpCompilationContext(
                assemblyName,
                targetNamespace,
                assemblyMetadataReferences: metadataReferences
            );
        }
    }
}