using System;
using System.Collections.Generic;
using Light.GuardClauses;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Light.Json.CodeGeneration
{
    public sealed class CompilationContext
    {
        private readonly Dictionary<string, MetadataReference> _metadataReferences;
        private readonly List<SyntaxTree> _syntaxTrees;

        public CompilationContext(string targetAssemblyName,
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

        public CompilationContext AddMetadataReferencesForType(Type type)
        {
            _metadataReferences.AddMetadataReferencesFromType(type);
            return this;
        }

        public CompilationContext AddSyntaxTree(SyntaxTree syntaxTree)
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

        public static CompilationContext CreateDefault(string targetNamespace, string? assemblyName = null)
        {
            assemblyName ??= targetNamespace;
            var metadataReferences = new Dictionary<string, MetadataReference>();
            metadataReferences.AddMetadataReferencesFromType(typeof(object));
            metadataReferences.AddMetadataReferencesFromType(typeof(JsonSerializer));
            return new CompilationContext(
                assemblyName,
                targetNamespace,
                assemblyMetadataReferences: metadataReferences
            );
        }
    }
}