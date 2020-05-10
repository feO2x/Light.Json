namespace Light.Json.Contracts
{
    public interface ISerializationContract { }
    public interface ISerializationContract<T> : ISerializeOnlyContract<T>, IDeserializeOnlyContract<T> { }
}