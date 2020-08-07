using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Light.GuardClauses;

namespace Light.Json.Contracts
{
    public sealed class DynamicContractProvider : IExtendedContractProvider
    {
        private readonly ICompiledContractProviderFactory _contractProviderFactory;
        private IExtendedContractProvider _compiledContractProvider;

        public DynamicContractProvider(IExtendedContractProvider compiledContractProvider, ICompiledContractProviderFactory contractProviderFactory)
        {
            _compiledContractProvider = compiledContractProvider.MustNotBeNull(nameof(compiledContractProvider));
            _contractProviderFactory = contractProviderFactory.MustNotBeNull(nameof(contractProviderFactory));
        }

        public bool TryGetContract<TContract>(TypeKey typeKey, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract =>
            _compiledContractProvider.TryGetContract(typeKey, out contract) || CompileNewContractProvider(typeKey, out contract);

        public bool TryGetContract<TType, TContract>([NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract =>
            _compiledContractProvider.TryGetContract<TType, TContract>(out contract) || CompileNewContractProvider(typeof(TType), out contract);

        public bool TryGetContract<TType, TContract>(string contractKey, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract =>
            _compiledContractProvider.TryGetContract<TType, TContract>(contractKey, out contract) || CompileNewContractProvider(new TypeKey(typeof(TType), contractKey), out contract);

        public bool TryGetContract<TType, TContract>(TType instance, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract =>
            _compiledContractProvider.TryGetContract(instance, out contract) || CompileNewContractProvider(instance!.GetType(), out contract);

        public bool TryGetContract<TType, TContract>(TType instance, string contractKey, [NotNullWhen(true)] out TContract? contract) where TContract : class, ISerializationContract =>
            _compiledContractProvider.TryGetContract(instance, contractKey, out contract) || CompileNewContractProvider(instance!.GetType(), out contract);

        private bool CompileNewContractProvider<TContract>(TypeKey typeKey, [NotNullWhen(true)] out TContract? contract)
            where TContract : class, ISerializationContract
        {
            var (contractProvider, serializationContract) = _contractProviderFactory.CompileNewContractProvider(typeKey);
            _compiledContractProvider = contractProvider;
            if (!(serializationContract is TContract castContract))
                throw new SerializationException($"Could not compile new contract provider for contract \"{typeKey}\".");

            contract = castContract;
            return true;
        }
    }
}