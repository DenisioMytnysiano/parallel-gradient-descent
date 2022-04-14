namespace PGD.Core.Solvers.Options
{
    public class ParallelMiniBatchGradientDescentOptions: GradientDescentOptions
    {
        public int ThreadEpochs { get; set; }

        public int NumThreads { get; set; }
    }
}