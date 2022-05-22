using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public ParallelMiniBatchGradientDescentSolver(IOptions<ParallelMiniBatchGradientDescentOptions> options)
        {
            _options = options.Value;
        }

        public Vector<double> Solve(IModel model, Matrix<double> x, Vector<double> y)
        {
            var gradients = new ConcurrentBag<ModelParameter>();
            var losses = Vector<double>.Build.Dense(_options.Epochs);
            var chunks = DataUtils.GetChunks(x, y, _options.NumThreads);
            var datasetSize = x.ColumnCount;
            for (var i = 0; i < _options.Epochs; i++)
            {
                ComputeGradientsParallel(model, gradients, chunks, datasetSize);
                UpdateParameters(model, gradients);
                losses[i] = model.ComputeLoss(x, y);
                gradients.Clear();
            }
            return losses;
        }

        private void PopulateGradients(IModel model, ConcurrentBag<ModelParameter> gradients, Matrix<double> input,
            Vector<double> target, int datasetSize, CountdownEvent countdownEvent = null)
        {
            var grads = model.ComputeGradients(input, target);
            foreach (var (parameterName, gradientValues) in grads)
                gradients.Add(new ModelParameter
                {
                    Name = parameterName,
                    Values = gradientValues * input.ColumnCount / datasetSize
                });

            if (countdownEvent != null)
                countdownEvent.Signal();
        }

        private void ComputeGradientsParallel(IModel model, ConcurrentBag<ModelParameter> gradients,
            IEnumerable<(Matrix<double> X, Vector<double> Y)> chunks, int datasetSize)
        {
            var countDown = new CountdownEvent(chunks.Count());
            foreach (var (x, y) in chunks)
                ThreadPool.QueueUserWorkItem(state => PopulateGradients(model, gradients, x, y, datasetSize, countDown));
            countDown.Wait();
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