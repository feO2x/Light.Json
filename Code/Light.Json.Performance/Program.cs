using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace Light.Json.Performance
{
    public static class Program
    {
        public static IConfig CreateDefaultConfig() =>
            DefaultConfig.Instance
                         .AddJob(Job.Default.WithRuntime(CoreRuntime.Core31))
                         .AddJob(Job.Default.WithRuntime(ClrRuntime.Net48))
                         .AddExporter(MarkdownExporter.Default)
                         .AddDiagnoser(MemoryDiagnoser.Default);

        public static void Main(string[] args) =>
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                             .Run(args, CreateDefaultConfig());
    }
}