using BenchmarkDotNet.Attributes;

namespace Light.Json.Performance.FrameworkExtensionsBenchmarks
{
    public class CharIsDigitBenchmark
    {
        public char Digit = '4';

        [Benchmark(Baseline = true)]
        public bool CharIsDigit()
        {
            return char.IsDigit(Digit);
        }

        [Benchmark]
        public bool IsDigitWithRangeCheck()
        {
            return IsDigitRange(Digit);
        }

        [Benchmark]
        public bool IsDigitWithSwitch()
        {
            return IsDigitSwitch(Digit);
        }

        private static bool IsDigitRange(char character)
        {
            return character >= '0' && character <= '9';
        }

        private static bool IsDigitSwitch(char character)
        {
            switch (character)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return true;
                default:
                    return false;
            }
        }

    }
}