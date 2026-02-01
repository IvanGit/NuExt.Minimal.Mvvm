using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Minimal.Mvvm;
using Minimal.Mvvm.Benchmarks;

namespace NuExt.System.Benchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

            var version = typeof(AsyncCommand).Assembly
                .GetName().Version?.ToString() ?? "1.0.0";
            var config = DefaultConfig.Instance
                .WithArtifactsPath($@"{version}")
                .WithOption(ConfigOptions.DisableOptimizationsValidator, true);
            BenchmarkRunner.Run<AsyncCommandBenchmarks>(config);
            Console.ReadKey();
        }
    }
}
