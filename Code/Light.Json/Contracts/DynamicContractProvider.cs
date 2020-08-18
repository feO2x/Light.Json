using Light.GuardClauses;

namespace Light.Json.Contracts
{
    public sealed class DynamicContractProvider : IContractProvider
    {
        private readonly IDictionaryContractProviderFactory _contractProviderFactory;
        private IDictionaryContractProvider _contractProvider;

        public DynamicContractProvider(IDictionaryContractProvider contractProvider, IDictionaryContractProviderFactory contractProviderFactory)
        {
            _contractProvider = contractProvider.MustNotBeNull(nameof(contractProvider));
            _contractProviderFactory = contractProviderFactory.MustNotBeNull(nameof(contractProviderFactory));
        }

        public TContract GetContract<TType, TContract>(string? contractKey)
            where TContract : class, ISerializationContract =>
            _contractProvider.TryGetContract<TType, TContract>(contractKey, out var contract) ? contract : TryExtendContractProvider<TType, TContract>(contractKey);

        public TContract GetContract<TType, TContract>(TType instance, string? contractKey)
            where TContract : class, ISerializationContract =>
            _contractProvider.TryGetContract<TType, TContract>(instance, contractKey, out var contract) ? contract : TryExtendContractProvider<TType, TContract>(instance, contractKey);

        private TContract TryExtendContractProvider<TType, TContract>(string? contractKey)
            where TContract : class, ISerializationContract
        {
            var currentProvider = _contractProvider;
            lock (_contractProviderFactory)
            {
                var possiblyExchangedProvider = _contractProvider;
                if (!ReferenceEquals(currentProvider, possiblyExchangedProvider) && possiblyExchangedProvider.TryGetContract<TType, TContract>(contractKey, out var contract))
                    return contract;

                var (newProvider, newContract) = _contractProviderFactory.ExtendContractProvider(currentProvider, new TypeKey(typeof(TType), contractKey));
                _contractProvider = newProvider;
                return (TContract) newContract;
            }
        }

        private TContract TryExtendContractProvider<TType, TContract>(TType instance, string? contractKey)
            where TContract : class, ISerializationContract
        {
            var currentProvider = _contractProvider;
            lock (_contractProviderFactory)
            {
                var possiblyExchangedProvider = _contractProvider;
                if (!ReferenceEquals(currentProvider, possiblyExchangedProvider) && possiblyExchangedProvider.TryGetContract<TType, TContract>(instance, contractKey, out var contract))
                    return contract;

                var targetType = instance?.GetType() ?? typeof(TType);
                var (newProvider, newContract) = _contractProviderFactory.ExtendContractProvider(currentProvider, new TypeKey(targetType, contractKey));
                _contractProvider = newProvider;
                return (TContract) newContract;
            }
        }
    }
}