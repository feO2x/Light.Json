using System.Diagnostics.CodeAnalysis;
using Light.Json.Contracts;

namespace Light.Json.Deserialization.Parsing
{
    public interface ITypeParserProvider
    {
        bool TryGetTypeParser<T>(TypeKey typeKey, [NotNullWhen(true)] out ITypeParser<T>? typeParser);
    }
}