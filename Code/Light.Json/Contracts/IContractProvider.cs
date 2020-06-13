using System.Diagnostics.CodeAnalysis;

namespace Light.Json.Contracts
{
    public interface IContractProvider
    {
        bool TryGetContract<TContract>(TypeKey typeKey, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract;
    }
}