using CommandLine;
using PGD.Core.Parsers.Implementation;
using PGD.Core.Solvers.Implementation;

namespace PGD.Cli
{
    public class CliOptions
    {
        [Option("solver-type", Required = false, HelpText = "Type of the algorithm (iterative or parallel).",
            Default = SolverType.IterativeGradientDescent)]
        public SolverType SolverType { get; set; }

        [Option("file-type", Required = false, HelpText = "Type of the file containing the input data.",
            Default = ParserType.Csv)]
        public ParserType ParserType { get; set; }

        [Option('f', "file-path", Required = true, HelpText = "Path to the input data.")]
        public string FilePath { get; set; }

        [Option('r', "learning-rate", Required = false, HelpText = "Learning rate.", Default = 0.9f)]
        public double LearningRate { get; set; }

        [Option('e', "epochs", Required = false, HelpText = "Number of epochs.", Default = 100)]
        public int Epochs { get; set; }

        [Option("thread-num", Required = false, HelpText = "Number of threads in case of parallel algorithm.",
            Default = 12)]
        public int ThreadNum { get; set; }

        [Option("thread-epochs", Required = false,
            HelpText = "Number of epochs per thread in case of parallel algorithm.", Default = 10)]
        public int ThreadEpochs { get; set; }
    }
}