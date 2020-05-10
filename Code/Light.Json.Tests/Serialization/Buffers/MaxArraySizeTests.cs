using System;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Light.Json.Tests.Serialization.Buffers
{
    public sealed class MaxArraySizeTests
    {
        private const string SkipReason = "This test triangulates the maximum array size.";
        private readonly ITestOutputHelper _output;

        public MaxArraySizeTests(ITestOutputHelper output) =>
            _output = output;

        [Fact(Skip = SkipReason)]
        public void DetermineCharMaximumArraySize() => DetermineArraySize<char>();

        [Fact(Skip = SkipReason)]
        public void DetermineByteMaximumArraySize() => DetermineArraySize<byte>();

        private void DetermineArraySize<T>()
        {
            var is64BitProcess = IntPtr.Size == 8;
            var lowerLimit = 1024 * 1024;
            var upperLimit = int.MaxValue;
            var currentSize = lowerLimit;

            _output.WriteLine($"Determining maximum array size of \"{typeof(T)}\"...");
            _output.WriteLine($"This is a {(is64BitProcess ? 64 : 32)}-Bit process");
            _output.WriteLine($"Initial lower limit is {lowerLimit:N0}");
            _output.WriteLine($"Initial upper limit is {upperLimit:N0}");

            Run:
            try
            {
                _output.WriteLine($"Checking size {currentSize:N0}...");
                CheckArraySize<T>(currentSize);
                _output.WriteLine($"Them maximum array size for \"{typeof(T)}\" is {currentSize:N0}.");
            }
            catch (OutOfMemoryException)
            {
                upperLimit = currentSize;
                currentSize -= (upperLimit - lowerLimit) / 2;
                goto Run;
            }
            catch (Exception)
            {
                lowerLimit = currentSize;
                currentSize += (upperLimit - lowerLimit) / 2;
                goto Run;
            }

        }

        private static void CheckArraySize<T>(int size)
        {
            var _ = new T[size];

            Action tooLarge = () =>
            {
                // ReSharper disable once VariableHidesOuterVariable
                var _ = new T[size + 1];
            };

            tooLarge.Should().Throw<OutOfMemoryException>();
        }
    }
}