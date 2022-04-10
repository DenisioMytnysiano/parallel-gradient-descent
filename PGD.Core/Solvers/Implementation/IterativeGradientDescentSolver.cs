using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using PGD.Core.Models.Interfaces;
using PGD.Core.Solvers.Interfaces;

namespace PGD.Core.Solvers.Implementation
{
    public class IterativeGradientDescentSolver : IGradientDescentSolver
    {
        private readonly IModel _model;
        private readonly double _lr;
        private readonly int _epochs;

        public IterativeGradientDescentSolver(IModel model, double lr, int epochs)
        {
            _model = model;
            _lr = lr;
            _epochs = epochs;
        }

        public Vector<double> Solve(Matrix<double> x, Vector<double> y)
        {
            var losses = Vector<double>.Build.Dense(_epochs);
            for (var i = 0; i < _epochs; i++)
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
                    parameterValue - _lr * gradients.GetValueOrDefault(parameterName));
        }
    }
}