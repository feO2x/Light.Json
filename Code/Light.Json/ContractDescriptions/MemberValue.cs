namespace Light.Json.ContractDescriptions
{
    public sealed class MemberValue
    {
        public MemberValue(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }

        public string Type { get; }
    }
}