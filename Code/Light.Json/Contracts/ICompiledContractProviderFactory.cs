namespace Light.Json.Contracts
{
    public interface ICompiledContractProviderFactory
    {
        (IExtendedContractProvider contractProvider, ISerializationContract contract)  CompileNewContractProvider(TypeKey typeKey);
    }
}