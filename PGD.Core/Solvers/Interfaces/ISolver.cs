using MathNet.Numerics.LinearAlgebra;
using PGD.Core.Models.Interfaces;

namespace PGD.Core.Solvers.Interfaces
{
    public interface IGradientDescentSolver
    {
        public Vector<double> Solve(IModel model, Matrix<double> x, Vector<double> y);
    }
}