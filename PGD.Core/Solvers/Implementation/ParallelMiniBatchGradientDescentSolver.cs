using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.Options;
using PGD.Core.Models.Interfaces;
using PGD.Core.Models.Shared;
using PGD.Core.Solvers.Interfaces;
using PGD.Core.Solvers.Options;
using PGD.Core.Utils;

namespace PGD.Core.Solvers.Implementation
{
    public class ParallelMiniBatchGradientDescentSolver : IGradientDescentSolver
    {
        private readonly ParallelMiniBatchGradientDescentOptions _options;
        private readonly IModel _model;

        public ParallelMiniBatchGradientDescentSolver(IModel model,
            IOptions<ParallelMiniBatchGradientDescentOptions> options)
        {
            _model = model;
            _options = options.Value;
        }

        public Vector<double> Solve(Matrix<double> x, Vector<double> y)
        {
            var gradients = new ConcurrentBag<ModelParameter>();
            var losses = Vector<double>.Build.Dense(_options.Epochs / _options.ThreadEpochs);
            var chunks = DataUtils.GetChunks(x, y, _options.NumThreads);
            for (var i = 0; i < _options.Epochs / _options.ThreadEpochs; i++)
            {
                ComputeGradientsParallel(gradients, chunks);
                UpdateParameters(_model, gradients);
                losses[i] = _model.ComputeLoss(x, y);
            }

            return losses;
        }

        private Task PopulateGradients(ConcurrentBag<ModelParameter> gradients, Matrix<double> input,
            Vector<double> target)
        {
            var grads = _model.ComputeGradients(input, target);
            foreach (var (parameterName, gradientValues) in grads)
                gradients.Add(new ModelParameter
                {
                    Name = parameterName,
                    Values = gradientValues
                });
            return Task.CompletedTask;
        }

        private void ComputeGradientsParallel(ConcurrentBag<ModelParameter> gradients,
            IEnumerable<(Matrix<double> X, Vector<double> Y)> chunks)
        {
            IEnumerable<Task> tasks = new List<Task>();
            foreach (var (x, y) in chunks)
                tasks.Append(Task.Factory.StartNew(
                    () => PopulateGradients(gradients, x, y)));

            Task.WhenAll(tasks);
        }

        private void UpdateParameters(IModel model, IEnumerable<ModelParameter> gradients)
        {
            if (gradients == null) throw new ArgumentNullException(nameof(gradients));
            foreach (var (parameterName, parameterValue) in gradients)
                model.UpdateParameter(parameterName,
                    model.GetParameter(parameterName).Values - _options.LearningRate * parameterValue);
        }
    }
}