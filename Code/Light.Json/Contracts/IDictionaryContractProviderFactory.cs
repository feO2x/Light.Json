namespace Light.Json.Contracts
{
    public interface IDictionaryContractProviderFactory
    {
        (IDictionaryContractProvider contractProvider, ISerializationContract newContract)  ExtendContractProvider(IDictionaryContractProvider currentProvider, TypeKey typeKeyOfNewContract);
    }
}