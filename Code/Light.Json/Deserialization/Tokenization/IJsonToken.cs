using System.Runtime.Serialization;
using Light.Json.Contracts;

namespace Light.Json.Deserialization.Tokenization
{
    public interface IJsonToken
    {
        JsonTokenType Type { get; }
        int Line { get; }
        int Position { get; }

        bool Equals(in ContractConstant constant);
    }

    public static class JsonTokenExtensions
    {
        public static void MustBeOfType<TJsonToken>(this TJsonToken token, JsonTokenType expectedType)
            where TJsonToken : struct, IJsonToken
        {
            if (token.Type == expectedType)
                return;

            throw new SerializationException($"Expected token {token.ToString()} at line {token.Line} position {token.Position} to have type {expectedType}, but it actually has type {token.Type}.");
        }
    }
}