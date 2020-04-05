using System.Diagnostics.CodeAnalysis;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Parsing
{
    public interface ITypeParserProvider 
    {
        bool TryGetTypeParser<T>(TypeKey typeKey, [NotNullWhen(true)] out ITypeParser<T>? typeParser);
    }
}