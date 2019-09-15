using System;
using System.Collections.Generic;
using Light.GuardClauses;

namespace Light.Json.FrameworkExtensions
{
    public static class CollectionExtensions
    {
        public static T[] ToSortedArray<T>(this HashSet<T> hashSet)
        {
            hashSet.MustNotBeNull(nameof(hashSet));

            if (hashSet.Count == 0)
                return Array.Empty<T>();

            var array = new T[hashSet.Count];
            hashSet.CopyTo(array);
            Array.Sort(array);
            return array;
        }
    }
}