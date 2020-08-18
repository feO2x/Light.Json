using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Light.GuardClauses;

namespace Light.Json.Contracts
{
    public abstract class BaseImmutableContractProvider : IContractProvider
    {
        protected BaseImmutableContractProvider(Dictionary<TypeKey, ISerializationContract> serializationContracts)
        {
            SerializationContracts = serializationContracts.MustNotBeNull(nameof(serializationContracts));
        }

        protected Dictionary<TypeKey, ISerializationContract> SerializationContracts { get; }

        public bool TryGetContract<TContract>(TypeKey typeKey, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract
        {
            if (SerializationContracts.TryGetValue(typeKey, out var serializationContract))
            {
                contract = serializationContract as TContract;
                return contract != null;
            }

            contract = default;
            return false;
        }
    }

    public sealed class ImmutableContractProvider : BaseImmutableContractProvider
    {

        public ImmutableContractProvider(Dictionary<TypeKey, ISerializationContract> serializationContracts)
            : base(serializationContracts) { }
    }
}