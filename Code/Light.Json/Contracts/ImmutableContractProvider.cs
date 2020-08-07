using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Light.GuardClauses;

namespace Light.Json.Contracts
{
    public sealed class ImmutableContractProvider : IContractProvider
    {
        private readonly Dictionary<TypeKey, ISerializationContract> _serializationContracts;

        public ImmutableContractProvider(Dictionary<TypeKey, ISerializationContract> serializationContracts) =>
            _serializationContracts = serializationContracts.MustNotBeNull(nameof(serializationContracts));

        public bool TryGetContract<TContract>(TypeKey typeKey, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract
        {
            if (_serializationContracts.TryGetValue(typeKey, out var serializationContract) &&
                serializationContract is TContract serializeOnlyContract)
            {
                contract = serializeOnlyContract;
                return true;
            }

            contract = default;
            return false;
        }
    }
}