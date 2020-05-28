using System;
using BenchmarkDotNet.Attributes;

namespace Light.Json.Performance.LowLevelWriting
{
    public class CopyUtf16Benchmark
    {
        public char[] Array1;
        public char[] Array2;

        [Params(1, 2, 3, 5, 10, 50, 100, 1000, 10_000, 40_000, 80_000, 256_000, 512_000, 1_000_000)]
        public int ArraySize { get; set; }

        [GlobalSetup]
        public void SetupArrays()
        {
            var random = new Random(42);
            Array1 = new char[ArraySize];
            for (var i = 0; i < ArraySize; i++)
            {
                Array1[i] = (char) random.Next(char.MinValue, char.MaxValue);
            }

            Array2 = new char[ArraySize];
        }

        [Benchmark(Baseline = true)]
        public char[] ForLoop()
        {
            for (var i = 0; i < Array1.Length; i++)
            {
                Array2[i] = Array1[i];
            }

            return Array2;
        }

        [Benchmark]
        public char[] CopyTo()
        {
            Array1.CopyTo(Array2, 0);
            return Array2;
        }

        [Benchmark]
        public char[] BufferBlockCopy()
        {
            Buffer.BlockCopy(Array1, 0, Array2, 0, Buffer.ByteLength(Array1));
            return Array2;
        }
    }
}