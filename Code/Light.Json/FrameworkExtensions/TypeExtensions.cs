using System;
using System.Collections.Generic;

namespace Light.Json.FrameworkExtensions
{
    public static class TypeExtensions
    {
        private static readonly Dictionary<Type, string> CSharpBuiltInTypes =
            new Dictionary<Type, string>
            {
                [typeof(bool)] = "bool",
                [typeof(int)] = "int",
                [typeof(short)] = "short",
                [typeof(long)] = "long",
                [typeof(uint)] = "uint",
                [typeof(ushort)] = "ushort",
                [typeof(ulong)] = "ulong",
                [typeof(byte)] = "byte",
                [typeof(sbyte)] = "sbyte",
                [typeof(double)] = "double",
                [typeof(float)] = "float",
                [typeof(decimal)] = "decimal",
                [typeof(char)] = "char",
                [typeof(string)] = "string",
                [typeof(object)] = "object"
            };

        public static bool TryGetCSharpBuiltInTypeAlias(this Type type, out string builtInTypeAlias) =>
            CSharpBuiltInTypes.TryGetValue(type, out builtInTypeAlias);
    }
}