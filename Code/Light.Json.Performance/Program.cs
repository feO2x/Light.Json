using System.IO;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

namespace Light.Json.Performance
{
    public static class Program
    {
        public static void Main(string[] args) =>
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                             .Run(args, CreateDefaultConfig());

        public static IConfig CreateDefaultConfig()
        {
            var defaultConfig = DefaultConfig.Instance;
            return ManualConfig.CreateEmpty()
                               .AddJob(Job.Default.WithRuntime(CoreRuntime.Core31))
                               .AddAnalyser(defaultConfig.GetAnalysers().ToArray())
                               .AddValidator(defaultConfig.GetValidators().ToArray())
                               .AddColumnProvider(defaultConfig.GetColumnProviders().ToArray())
                               .AddExporter(MarkdownExporter.Default)
                               .AddDiagnoser(MemoryDiagnoser.Default)
                               .AddLogger(ConsoleLogger.Default)
                               .WithArtifactsPath(FindLightJsonPerformanceDirectory())
                               .DontOverwriteResults();
        }

        private static string FindLightJsonPerformanceDirectory()
        {
            var currentDirectory = new DirectoryInfo(".");
            var targetDirectoryName = typeof(Program).Namespace;
            while (true)
            {
                if (currentDirectory.Name == targetDirectoryName)
                    return Path.Combine(currentDirectory.FullName, "BenchmarkDotNet.Artifacts");
                currentDirectory = currentDirectory.Parent ?? throw new DirectoryNotFoundException($"Could not find parent directory \"{targetDirectoryName}\".");
            }
        }
    }
}