using MathNet.Numerics.LinearAlgebra;

namespace PGD.Core.Solvers.Interfaces
{
    public interface IGradientDescentSolver
    {
        public Vector<double> Solve(Matrix<double> x, Vector<double> y);
    }
}