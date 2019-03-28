using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Light.Json.Performance
{
    public static class Program
    {
        public static IConfig CreateDefaultConfig() =>
            DefaultConfig.Instance
                         .With(Job.Core)
                         .With(MemoryDiagnoser.Default);

        public static void Main(string[] args) =>
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                             .Run(args, CreateDefaultConfig());
    }
}