using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Channels;
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
using PGD.Core.Utils;

namespace PGD.Cli
{
    public static class CliMain
    {
        public static IConfiguration Configuration { get; set; }
        public static CliOptions GlobalOptions { get; set; }

        private static void Main(string[] args)
        {
            GlobalOptions = Parser.Default.ParseArguments<CliOptions>(args).Value;
            RunOptimization();
        }

        private static void RunOptimization()
        {
            InitializeConfiguration();
            
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();

            var parser = host.Services.GetRequiredService<IParser>();
            var optimizationAlgorithm = host.Services.GetRequiredService<IGradientDescentSolver>();
            var (input, target) = parser.Parse(GlobalOptions.FilePath);
            //var (input, target) = DataUtils.GetRandomInput(50000, 5, 12);
            var model = host.Services.GetRequiredService<IModel>();
            model.Initialize(input.ColumnCount);
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var losses = optimizationAlgorithm.Solve(model, input, target);
            stopwatch.Stop();
            Console.WriteLine($"Time elapsed:{stopwatch.Elapsed}");
            Console.WriteLine($"Loss value: {losses.Last()}");
        }

        private static void InitializeConfiguration()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .Build();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
        {
            serviceCollection.AddOptions();
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

        private static void AddOptions(this IServiceCollection serviceCollection)
        {
            serviceCollection.Configure<CsvParserOptions>(Configuration.GetSection("CsvOptions"));
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