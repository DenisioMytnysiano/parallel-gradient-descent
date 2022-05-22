using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using PGD.Core.Models.Interfaces;
using PGD.Core.Models.Shared;

namespace PGD.Core.Models.Implementation.LogisticRegression
{
    public class LogisticRegressionModel : IModel
    {
        private const string Bias = "Bias";
        private const string Weights = "Weights";
        private readonly ModelParametersList _modelParameters;

        public LogisticRegressionModel()
        {
            _modelParameters = new ModelParametersList(new List<ModelParameter>());
        }

        public Vector<double> Predict(Matrix<double> x)
        {
            var result = new List<double>();
            foreach (var row in x.EnumerateRows())
                result.Add(1 / (1 + Math.Exp(-_modelParameters.GetModelParameter(Weights).Values.DotProduct(row) -
                                             _modelParameters.GetModelParameter(Bias).Values[0])));
            return Vector<double>.Build.DenseOfEnumerable(result);
        }

        public Dictionary<string, Vector<double>> ComputeGradients(Matrix<double> x, Vector<double> y)
        {
            var prediction = Predict(x);
            return new Dictionary<string, Vector<double>>
            {
                [Bias] = Vector<double>.Build.DenseOfEnumerable(new List<double> {(prediction - y).Average()}),
                [Weights] = (prediction - y) * x
            };
        }

        private void InitializeParameter(string parameterName, Vector<double> values)
        {
            _modelParameters.CreateModelParameter(parameterName, values);
        }

        public void UpdateParameter(string parameterName, Vector<double> values)
        {
            _modelParameters.UpdateModelParameter(parameterName, values);
        }

        public ModelParameter GetParameter(string parameterName)
        {
            return _modelParameters.GetModelParameter(parameterName);
        }

        public ModelParametersList GetParameters()
        {
            return _modelParameters;
        }

        public void Initialize(int dimensionsCount)
        {
            InitializeParameter(Bias, Vector<double>.Build.Dense(1));
            InitializeParameter(Weights, Vector<double>.Build.Dense(dimensionsCount));
        }

        public double ComputeLoss(Matrix<double> x, Vector<double> target)
        {
            var eps = 1e-5;
            var prediction = Predict(x).PointwiseMinimum(1 - eps).PointwiseMaximum(eps);
            return -(target.PointwiseMultiply(prediction.PointwiseLog()) +
                     (1 - target).PointwiseMultiply((1 - prediction).PointwiseLog())).Average();
        }
    }
}