using System;
using Light.GuardClauses;
using Light.Json.Buffers;
using Light.Json.CodeGeneration.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Light.Json.CodeGeneration
{
    public sealed class ContractContext
    {
        public ContractContext(string contractType,
                               string subjectParameterName,
                               CompilationContext compilationContext,
                               Document document,
                               UsingStatementsBlock usingStatementsBlock,
                               Class contractClass,
                               CodeSink codeSink)
        {
            ContractType = contractType.MustNotBeNullOrWhiteSpace(nameof(contractType));
            SubjectParameterName = subjectParameterName.MustNotBeNullOrWhiteSpace(nameof(subjectParameterName));
            CompilationContext = compilationContext.MustNotBeNull(nameof(compilationContext));
            UsingStatementsBlock = usingStatementsBlock.MustNotBeNull(nameof(usingStatementsBlock));
            ContractClass = contractClass.MustNotBeNull(nameof(contractClass));
            Document = document.MustNotBeNull(nameof(document));
            CodeSink = codeSink.MustNotBeNull(nameof(codeSink));
        }

        public string ContractType { get; }

        public string SubjectParameterName { get; }

        private CompilationContext CompilationContext { get; }

        private Document Document { get; }

        private UsingStatementsBlock UsingStatementsBlock { get; }

        private Class ContractClass { get; }

        private CodeSink CodeSink { get; }


        public ContractContext AddUsingStatementAndMetadataReferenceIfNecessary(Type type)
        {
            type.MustNotBeNull(nameof(type));
            UsingStatementsBlock.Add(type.Namespace);
            CompilationContext.AddMetadataReferencesForType(type);
            return this;
        }

        public ContractContext AddConstantValueField(string name, string? initializationExpression = null)
        {
            var field = new Field(nameof(ConstantValue), name)
            {
                Modifiers = Modifiers.PublicReadonly,
                InitializationExpression = initializationExpression
            };
            ContractClass.AddChildNode(field);
            return this;
        }

        public Constructor CreateDefaultConstructor() => new Constructor(ContractClass.Name);

        public ContractContext AddConstructor(Constructor constructor)
        {
            constructor.MustNotBeNull(nameof(constructor));
            ContractClass.AddChildNode(constructor);
            return this;
        }

        public Method CreateSerializeMethod() =>
            new Method("Serialize")
            {
                Modifiers = Modifiers.PublicOverride,
                ReturnType = "void",
                GenericParameters = "TJsonWriter",
                ParameterList = ContractType + " " + SubjectParameterName + ", SerializationContext context, ref TJsonWriter writer"
            };

        public Method CreateSerializeFastMethod() =>
            new Method("SerializeFast")
            {
                Modifiers = Modifiers.Private,
                ReturnType = "void",
                GenericParameters = "TJsonWriter",
                ParameterList = ContractType + " " + SubjectParameterName + ", ref TJsonWriter writer",
                GenericConstraints = GenericConstraints.WhereTJsonWriterIsStructIJsonWriter
            };

        public Method CreateDeserializeMethod() =>
            new Method("Deserialize")
            {
                Modifiers = Modifiers.PublicOverride,
                ReturnType = ContractType,
                GenericParameters = "TJsonTokenizer, TJsonToken",
                ParameterList = "DeserializationContext context, ref TJsonTokenizer tokenizer"
            };

        public ContractContext AddMethod(Method method)
        {
            method.MustNotBeNull(nameof(method));
            ContractClass.AddChildNode(method);
            return this;
        }

        public CompilationContext FinishContract()
        {
            Document.WriteSyntax(CodeSink);
            var sourceCode = CodeSink.ToString();
            var syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            return CompilationContext.AddSyntaxTree(syntaxTree);
        }
    }
}