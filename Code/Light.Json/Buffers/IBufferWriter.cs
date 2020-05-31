using System.Threading.Tasks;

namespace Light.Json.Buffers
{
    public interface IBufferWriter
    {
        int CurrentIndex { get; }

        int EnsuredIndex { get; }

        Task EnsureCapacityFromCurrentIndexAsync(int numberOfBufferSlots);

        Task EnsureAdditionalCapacityAsync(int numberOfAdditionalBufferSlots);

        Task WriteCharacterAsync(char character);

        void WriteAscii(char character);

        Task WriteSurrogatePairAsync(char highSurrogate, char lowSurrogate);
    }
}