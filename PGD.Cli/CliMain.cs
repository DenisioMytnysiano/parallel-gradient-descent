using System;
using System.Diagnostics;
using System.Threading;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PGD.Core.Models.Implementation.LogisticRegression;
using PGD.Core.Models.Interfaces;
using PGD.Core.Parsers.Implementation;
using PGD.Core.Parsers.Implementation.Csv;
using PGD.Core.Parsers.Interfaces;
using PGD.Core.Solvers.Implementation;
using PGD.Core.Solvers.Interfaces;
using PGD.Core.Solvers.Options;

namespace PGD.Cli
{
    public static class CliMain
    {
        public static CliOptions GlobalOptions { get; set; }

        private static void Main(string[] args)
        {
            GlobalOptions = Parser.Default.ParseArguments<CliOptions>(args).Value;
            RunOptimization();
        }

        private static void RunOptimization()
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((ctx, sc) => ConfigureServices(sc, ctx.Configuration))
                .Build();

            if (ThreadPool.SetMinThreads(1, 1) &&
                ThreadPool.SetMaxThreads(GlobalOptions.ThreadNum, GlobalOptions.ThreadNum))
            {
                var parser = host.Services.GetRequiredService<IParser>();
                var optimizationAlgorithm = host.Services.GetRequiredService<IGradientDescentSolver>();
                var (input, target) = parser.Parse(GlobalOptions.FilePath);
                var model = host.Services.GetRequiredService<IModel>();
                model.Initialize(input.ColumnCount);
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var losses = optimizationAlgorithm.Solve(model, input, target);
                stopwatch.Stop();
                var i = 0;
                foreach (var loss in losses)
                {
                    Console.WriteLine($"Epoch {i}: {loss}");
                    i++;
                }

                Console.WriteLine($"Time elapsed:{stopwatch.Elapsed.TotalMilliseconds} ms.");
            }
            else
            {
                Console.WriteLine($"Failed to scale thread pool to {GlobalOptions.ThreadNum} threads.");
            }
        }

        private static void ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddOptions(configuration);
            serviceCollection.AddParser();
            serviceCollection.AddSolver();
            serviceCollection.AddModel();
        }

        private static void AddParser(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<CsvParser>();
            serviceCollection.AddSingleton<IParser>(sp =>
            {
                return GlobalOptions.ParserType switch
                {
                    ParserType.Csv => sp.GetRequiredService<CsvParser>(),
                    _ => throw new ArgumentOutOfRangeException(nameof(ParserType))
                };
            });
        }

        private static void AddSolver(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IterativeGradientDescentSolver>();
            serviceCollection.AddSingleton<ParallelMiniBatchGradientDescentSolver>();
            serviceCollection.AddSingleton<IGradientDescentSolver>(sp =>
            {
                return GlobalOptions.SolverType switch
                {
                    SolverType.IterativeGradientDescent => sp.GetRequiredService<IterativeGradientDescentSolver>(),
                    SolverType.ParallelGradientDescent =>
                        sp.GetRequiredService<ParallelMiniBatchGradientDescentSolver>(),
                    _ => throw new ArgumentOutOfRangeException(nameof(SolverType))
                };
            });
        }

        private static void AddModel(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IModel, LogisticRegressionModel>();
        }

        private static void AddOptions(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.Configure<CsvParserOptions>(configuration.GetSection("CsvOptions"));
            serviceCollection.Configure<GradientDescentOptions>(op =>
            {
                op.Epochs = GlobalOptions.Epochs;
                op.LearningRate = GlobalOptions.LearningRate;
            });
            serviceCollection.Configure<ParallelMiniBatchGradientDescentOptions>(op =>
            {
                op.Epochs = GlobalOptions.Epochs;
                op.LearningRate = GlobalOptions.LearningRate;
                op.NumThreads = GlobalOptions.ThreadNum;
            });
        }
    }
}