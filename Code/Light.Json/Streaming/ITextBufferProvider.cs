namespace Light.Json.Streaming
{
    public interface ITextBufferProvider
    {
        char[] RentBuffer();
        void ReturnBuffer(char[] buffer);
    }
}