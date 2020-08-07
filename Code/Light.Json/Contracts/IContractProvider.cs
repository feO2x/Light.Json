using System.Diagnostics.CodeAnalysis;

namespace Light.Json.Contracts
{
    public interface IContractProvider
    {
        bool TryGetContract<TContract>(TypeKey typeKey, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract;
    }

    public interface IExtendedContractProvider : IContractProvider
    {
        bool TryGetContract<TType, TContract>([NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract;

        bool TryGetContract<TType, TContract>(string contractKey, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract;

        bool TryGetContract<TType, TContract>(TType instance, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract;

        bool TryGetContract<TType, TContract>(TType instance, string contractKey, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract;
    }
}