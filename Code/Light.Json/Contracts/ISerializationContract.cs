namespace Light.Json.Contracts
{
    public interface ISerializationContract
    {
        TypeKey TypeKey { get; }
    }
    public interface ISerializationContract<T> : ISerializeOnlyContract<T>, IDeserializeOnlyContract<T> { }
}