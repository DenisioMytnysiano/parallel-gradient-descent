using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using PGD.Core.Models.Interfaces;
using PGD.Core.Solvers.Interfaces;

namespace PGD.Core.Solvers.Implementation
{
    public class ParallelMiniBatchGradientDescentSolver : IGradientDescentSolver
    {
        private readonly IModel _model;
        private readonly double _lr;
        private readonly int _epochs;
        private readonly int _threadEpochs;
        private readonly int _numThreads;

        public ParallelMiniBatchGradientDescentSolver(int threadEpochs, int numThreads, double lr, IModel model,
            int epochs)
        {
            _threadEpochs = threadEpochs;
            _numThreads = numThreads;
            _lr = lr;
            _model = model;
            _epochs = epochs;
        }

        public Vector<double> Solve(Matrix<double> x, Vector<double> y)
        {
            var losses = Vector<double>.Build.Dense(_epochs / _threadEpochs);
            var chunks = new List<Matrix<double>>();
            for (var i = 0; i < _epochs / _threadEpochs; i++)
            {
                var gradients = ComputeGradientsParallel(chunks);
            }

            return losses;
        }

        private Dictionary<string, Vector<double>> ComputeGradientsParallel(IEnumerable<Matrix<double>> chunks)
        {
            IEnumerable<Task> tasks = new List<Task>();
            foreach (var chunk in chunks)
            {
            }

            return new Dictionary<string, Vector<double>>();
        }

        private void UpdateParameters(IModel model, IDictionary<string, Vector<double>> gradients)
        {
            if (gradients == null) throw new ArgumentNullException(nameof(gradients));
            var modelParams = model.GetParameters();
            foreach (var (parameterName, parameterValue) in modelParams)
                model.UpdateParameter(parameterName,
                    parameterValue - _lr * gradients[parameterName]);
        }
    }
}