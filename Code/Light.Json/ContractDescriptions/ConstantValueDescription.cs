using Light.Json.Buffers;

namespace Light.Json.ContractDescriptions
{
    public sealed class ConstantValueDescription
    {
        public ConstantValueDescription(string name,
                                        ConstantValueType type,
                                        string initializationExpression,
                                        ConstantValue constantValue)
        {
            Name = name;
            Type = type;
            InitializationExpression = initializationExpression;
            ConstantValue = constantValue;
        }

        public string Name { get; }

        public ConstantValueType Type { get; }

        public string InitializationExpression { get; }

        public ConstantValue ConstantValue { get; }
    }
}