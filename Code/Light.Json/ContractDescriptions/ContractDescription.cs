using System.Collections.Generic;
using Light.GuardClauses;
using Light.Json.Contracts;

namespace Light.Json.ContractDescriptions
{
    public sealed class ContractDescription
    {
        public ContractDescription(string contractClassName,
                                   string targetTypeName,
                                   ContractKind contractKind,
                                   string targetNamespace,
                                   string? contractKey,
                                   List<ConstantValueDescription> constantValues,
                                   List<MemberValue> memberValues)
        {
            ContractClassName = contractClassName.MustNotBeNullOrWhiteSpace(nameof(contractClassName));
            TargetTypeName = targetTypeName.MustNotBeNullOrWhiteSpace(nameof(targetTypeName));
            ContractKind = contractKind.MustBeValidEnumValue(nameof(contractKind));
            TargetNamespace = targetNamespace.MustNotBeNullOrWhiteSpace(nameof(targetNamespace));
            ContractKey = contractKey;
            ConstantValues = constantValues;
            MemberValues = memberValues;
        }


        public string ContractClassName { get; }

        public string TargetTypeName { get; }

        public ContractKind ContractKind { get; }

        public string TargetNamespace { get; }

        public string? ContractKey { get; }

        public List<ConstantValueDescription> ConstantValues { get; }

        public List<MemberValue> MemberValues { get; }
    }
}