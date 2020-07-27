using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Light.Json.CodeGeneration.Syntax
{
    public static class CollectionExtensions
    {
        public static bool TryGetFirst<T>(this IList syntaxNodes, [NotNullWhen(true)] out T? node)
            where T : class
        {
            if (syntaxNodes == null)
                goto NoItemFound;

            for (var i = 0; i < syntaxNodes.Count; i++)
            {
                if (!(syntaxNodes[i] is T castNode))
                    continue;

                node = castNode;
                return true;
            }

            NoItemFound:
            node = default;
            return false;
        }
    }
}