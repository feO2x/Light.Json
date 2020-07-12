namespace Light.Json.Contracts
{
    public interface ISerializationContract
    {
        TypeKey TypeKey { get; }

        ContractKind Kind { get; }
    }

    public interface ISerializationContract<T> : ISerializeOnlyContract<T>, IDeserializeOnlyContract<T> { }
}