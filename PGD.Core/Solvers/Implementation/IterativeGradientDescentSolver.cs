using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.Options;
using PGD.Core.Models.Interfaces;
using PGD.Core.Solvers.Interfaces;
using PGD.Core.Solvers.Options;

namespace PGD.Core.Solvers.Implementation
{
    public class IterativeGradientDescentSolver : IGradientDescentSolver
    {
        private readonly IModel _model;
        private readonly GradientDescentOptions _options;

        public IterativeGradientDescentSolver(IModel model, IOptions<GradientDescentOptions> options)
        {
            _model = model;
            _options = options.Value;
        }

        public Vector<double> Solve(Matrix<double> x, Vector<double> y)
        {
            var losses = Vector<double>.Build.Dense(_options.Epochs);
            for (var i = 0; i < _options.Epochs; i++)
            {
                var gradients = _model.ComputeGradients(x, y);
                losses[i] = _model.ComputeLoss(x, y);
                UpdateParameters(_model, gradients);
            }

            return losses;
        }

        private void UpdateParameters(IModel model, Dictionary<string, Vector<double>> gradients)
        {
            if (gradients == null) throw new ArgumentNullException(nameof(gradients));
            var modelParams = model.GetParameters();
            foreach (var (parameterName, parameterValue) in modelParams)
                model.UpdateParameter(parameterName,
                    parameterValue - _options.LearningRate * gradients.GetValueOrDefault(parameterName));
        }
    }
}