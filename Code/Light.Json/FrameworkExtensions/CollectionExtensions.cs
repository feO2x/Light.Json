using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.Json.FrameworkExtensions
{
    public static class CollectionExtensions
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        public static bool TryPeek<T>(this Stack<T> stack, out T item)
        {
            if (stack.IsNullOrEmpty())
            {
                item = default;
                return false;
            }

            item = stack.Peek();
            return true;
        }
#pragma warning restore CS8601 // Possible null reference assignment.
    }
}