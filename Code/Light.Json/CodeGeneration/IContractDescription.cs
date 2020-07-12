namespace Light.Json.CodeGeneration
{
    public interface IContractDescription
    {
        string TargetNamespace { get; }

        string ContractClassName { get; }

        bool CanSerialize { get; }

        bool CanDeserialize { get; }
    }
}