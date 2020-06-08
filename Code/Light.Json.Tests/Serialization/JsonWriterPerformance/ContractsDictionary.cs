using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Light.GuardClauses;

namespace Light.Json.Tests.Serialization.JsonWriterPerformance
{
    public sealed class ContractsDictionary
    {
        private readonly BaseContract?[] _contracts;
        private readonly ulong _fastModuloMultiplier;

        public ContractsDictionary()
        {
            _contracts = new BaseContract?[31];
            if (Environment.Is64BitProcess)
                _fastModuloMultiplier = GetFastModuloMultiplier(31);
        }

        public ContractsDictionary Add(BaseContract contract)
        {
            contract.MustNotBeNull(nameof(contract));
            var currentInsertIndex = GetBucketIndex(contract.TypeKey.HashCode);
            var collisionCount = 0;
            do
            {
                if (currentInsertIndex >= (uint) _contracts.Length)
                    currentInsertIndex = 0;

                if (_contracts[currentInsertIndex] == null)
                {
                    _contracts[currentInsertIndex] = contract;
                    return this;
                }

                currentInsertIndex++;
                collisionCount++;
            } while (collisionCount <= _contracts.Length);

            throw new NotImplementedException();
        }

        public bool TryGetContract<TContract>(TypeKey typeKey, [NotNullWhen(true)] out TContract? contract)
            where TContract : class
        {
            var currentIndex = GetBucketIndex(typeKey.HashCode);
            var collisionCount = 0;
            do
            {
                if (currentIndex >= (uint) _contracts.Length)
                    currentIndex = 0;

                var currentContract = _contracts[currentIndex];
                if (currentContract == null)
                    goto NotFound;

                if (currentContract.TypeKey.HashCode == typeKey.HashCode &&
                    currentContract.TypeKey.Equals(typeKey) &&
                    currentContract is TContract castContract)
                {
                    contract = castContract;
                    return true;
                }

                currentIndex++;
                collisionCount++;
            } while (collisionCount < _contracts.Length);

            NotFound:
            contract = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private uint GetBucketIndex(uint hashCode)
        {
            if (Environment.Is64BitProcess)
                return FastModulo(hashCode, (uint) _contracts.Length, _fastModuloMultiplier);
            return hashCode % (uint) _contracts.Length;
        }

        private static ulong GetFastModuloMultiplier(uint divisor)
            => ulong.MaxValue / divisor + 1;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint FastModulo(uint hashCode, uint divisor, ulong multiplier)
        {
            var lowBits = multiplier * hashCode;
            var high = (uint) ((((ulong) (uint) lowBits * divisor >> 32) + (lowBits >> 32) * divisor) >> 32);
            return high;
        }
    }
}