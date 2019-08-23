using System;

namespace Light.Json.Parsing
{
    public interface ITokenParser
    {
        Type TargetType { get; }
    }
}