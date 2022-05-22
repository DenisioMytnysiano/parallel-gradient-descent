namespace PGD.Core.Solvers.Options
{
    public class ParallelMiniBatchGradientDescentOptions: GradientDescentOptions
    { 
        public int NumThreads { get; set; }
    }
}