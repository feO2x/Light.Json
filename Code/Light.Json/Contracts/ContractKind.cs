using System;

namespace Light.Json.Contracts
{
    [Flags]
    public enum ContractKind
    {
        SerializeOnly = 1,
        DeserializeOnly = 2,
        TwoWay = SerializeOnly | DeserializeOnly
    }
}