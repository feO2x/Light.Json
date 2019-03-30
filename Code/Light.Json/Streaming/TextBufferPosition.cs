using Light.GuardClauses;
using Light.Json.FrameworkExtensions;

namespace Light.Json.Streaming
{
    public readonly ref struct TextBufferPosition
    {
        public readonly char[] TargetBuffer;
        public readonly int Index;

        public TextBufferPosition(char[] targetBuffer, int index)
        {
            TargetBuffer = targetBuffer.MustNotBeNull(nameof(targetBuffer));
            Index = index.MustNotBeLessThan(0, nameof(index))
                         .MustNotBeGreaterThanOrEqualTo(targetBuffer.Length, nameof(index));
        }

        public bool Equals(in TextBufferPosition other) =>
            ReferenceEquals(TargetBuffer, other.TargetBuffer) &&
            Index == other.Index;

        public override bool Equals(object obj) =>
            throw BoxingNotSupported.CreateException();


        public override int GetHashCode() =>
            throw BoxingNotSupported.CreateException();

        public static bool operator ==(TextBufferPosition x, TextBufferPosition y) => x.Equals(y);
        public static bool operator !=(TextBufferPosition x, TextBufferPosition y) => !x.Equals(y);
    }
}