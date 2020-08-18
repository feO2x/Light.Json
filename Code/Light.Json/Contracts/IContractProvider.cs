using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Light.Json.Contracts
{
    public interface IContractProvider
    {
        TContract GetContract<TType, TContract>(string? contractKey)
            where TContract : class, ISerializationContract;

        TContract GetContract<TType, TContract>(TType instance, string? contractKey)
            where TContract : class, ISerializationContract;
    }

    public interface IDictionaryContractProvider : IContractProvider
    {
        IReadOnlyDictionary<TypeKey, ISerializationContract> Contracts { get; }

        bool TryGetContract<TType, TContract>(string? contractKey, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract;

        bool TryGetContract<TType, TContract>(TType instance, string? contractKey, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract;
    }
}