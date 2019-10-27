using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Light.Json.Performance
{
    public static class Program
    {
        public static IConfig CreateDefaultConfig() =>
            DefaultConfig.Instance
                         .With(Job.Default.With(CoreRuntime.Core22))
                         .With(Job.Default.With(CoreRuntime.Core30))
                         .With(Job.Default.With(ClrRuntime.Net48))
                         .With(MemoryDiagnoser.Default);

        public static void Main(string[] args) =>
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                             .Run(args, CreateDefaultConfig());
    }
}