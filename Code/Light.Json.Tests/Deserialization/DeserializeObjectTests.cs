using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Light.Json.Tests.Deserialization
{
    public static class DeserializeObjectTests
    {
        [Fact]
        public static void DeserializeMututableObjectWithPrimitiveProperties()
        {
            const string json = "{ value1: \"Foo\" }";
            
            throw new NotImplementedException();
        }
    }

    public class Dto
    {
        public string Value1 { get; set; }
    }
}
