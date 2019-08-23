using System;
using Light.GuardClauses;

namespace Light.Json.Parsing
{
    public abstract class Utf16Parser : ITokenParser
    {
        protected Utf16Parser(Type targetType) =>
            TargetType = targetType.MustNotBeNull(nameof(targetType));

        public Type TargetType { get; }

        public abstract object Parse(Utf16Context context);

        public override string ToString() => TargetType.ToString();
    }

    public abstract class Utf16Parser<T> : Utf16Parser
    {
        protected Utf16Parser(Type targetType = null) : base(targetType ?? typeof(T)) { }

        public override object Parse(Utf16Context context) => ParseGeneric(context);

        public abstract T ParseGeneric(Utf16Context context);
    }
}