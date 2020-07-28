using System.Collections.Generic;

namespace Light.Json.CodeGeneration
{
    public static class GenericConstraints
    {
        public static readonly List<string> WhereTJsonWriterIsStructIJsonWriter = new List<string>(1) { "where TJsonWriter : struct, IJsonWriter" };
    }
}