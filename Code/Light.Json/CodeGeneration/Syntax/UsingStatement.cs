using System;
using Light.GuardClauses;

namespace Light.Json.CodeGeneration.Syntax
{
    public readonly struct UsingStatement : ISyntaxWriter, IEquatable<UsingStatement>, IComparable<UsingStatement>
    {
        public UsingStatement(string @namespace) =>
            Namespace = @namespace.MustNotBeNullOrWhiteSpace(nameof(@namespace));

        public string Namespace { get; }

        public void WriteSyntax(CodeSink sink) => sink.WriteLine("using " + Namespace + ";");

        public bool Equals(UsingStatement other) => Namespace == other.Namespace;

        public override bool Equals(object? obj) => obj is UsingStatement other && Equals(other);

        public override int GetHashCode() => Namespace.GetHashCode();

        public static bool operator ==(UsingStatement left, UsingStatement right) => left.Equals(right);

        public static bool operator !=(UsingStatement left, UsingStatement right) => !left.Equals(right);

        public int CompareTo(UsingStatement other) =>
            string.Compare(Namespace, other.Namespace, StringComparison.Ordinal);
    }
}