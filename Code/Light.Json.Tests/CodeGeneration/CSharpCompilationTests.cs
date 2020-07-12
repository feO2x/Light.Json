using System.IO;
using FluentAssertions;
using Light.Json.CodeGeneration;
using Light.Json.Contracts;
using Light.Json.Tests.SerializationSubjects;
using Xunit;
using Xunit.Abstractions;

namespace Light.Json.Tests.CodeGeneration
{
    public sealed class CSharpCompilationTests
    {
        private readonly ITestOutputHelper _output;

        public CSharpCompilationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AddingSameMetadataReferenceTwice()
        {
            var context = CSharpCompilationContext.CreateDefault("SerializationContracts")
                                                  .AddMetadataReferencesForType(typeof(Person));
            context.CreateContract(typeof(Person), ContractKind.SerializeOnly).CreateSyntaxTreeAndAddToCompilationContext();

            var compilation = context.CreateCompilation();
            using var memoryStream = new MemoryStream();
            var result = compilation.Emit(memoryStream);

            foreach (var diagnostic in result.Diagnostics)
            {
                _output.WriteLine(diagnostic.ToString());
            }
            result.Success.Should().BeTrue();
        }
    }
}