using System;
using System.Collections.Generic;
using System.Text;
using Light.GuardClauses;
using Light.Json.CodeGeneration.Syntax;
using Light.Json.FrameworkExtensions;
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
                                  CSharpCompilationOptions? compilationOptions,
                                  Dictionary<string, MetadataReference>? assemblyMetadataReferences = null,
                                  List<SyntaxTree>? syntaxTrees = null)
        {
            TargetAssemblyName = targetAssemblyName.MustNotBeNullOrWhiteSpace(nameof(targetAssemblyName));
            TargetNamespace = targetNamespace.MustNotBeNullOrWhiteSpace(nameof(targetNamespace));
            CompilationOptions = compilationOptions.MustNotBeNull(nameof(compilationOptions));
            _metadataReferences = assemblyMetadataReferences ?? new Dictionary<string, MetadataReference>();
            _syntaxTrees = syntaxTrees ?? new List<SyntaxTree>();
        }

        public string TargetAssemblyName { get; }

        public string TargetNamespace { get; }

        public CSharpCompilationOptions? CompilationOptions { get; }

        private CodeSink CodeSink { get; } = new CodeSink(new StringBuilder());

        public CompilationContext AddMetadataReferencesForType(Type type)
        {
            _metadataReferences.AddMetadataReferencesFromType(type);
            return this;
        }

        public ContractContext CreateContract(Type contractType)
        {
            var usingStatements = new UsingStatementsBlock().Add("System")
                                                            .Add("Light.Json.Buffers")
                                                            .Add("Light.Json.Contracts")
                                                            .Add("Light.Json.Deserialization")
                                                            .Add("Light.Json.Deserialization.Tokenization")
                                                            .Add("Light.Json.Serialization")
                                                            .Add("Light.Json.Serialization.LowLevelWriter");
            var contractClass = new Class(contractType.Name + "Contract");
            var document = new Document().AddChildNode(usingStatements)
                                         .AddChildNode(new Namespace("SerializationContracts").AddChildNode(contractClass));
            return new ContractContext(
                contractType.Name,
                contractType.Name.LowerFirstCharacter(),
                this,
                document,
                usingStatements,
                contractClass,
                CodeSink.Reset()
            );
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
                CompilationOptions
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
                CreateDefaultCompilationOptions(),
                metadataReferences
            );
        }

        public static CSharpCompilationOptions CreateDefaultCompilationOptions() =>
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release);
    }
}