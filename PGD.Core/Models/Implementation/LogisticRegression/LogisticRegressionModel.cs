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
        private ModelParametersList _modelParameters;

        public LogisticRegressionModel()
        {
            UpdateParameter(Bias, Vector<double>.Build.Dense(1));
            UpdateParameter(Weights, Vector<double>.Build.Dense(10));
        }

        public Vector<double> Predict(Matrix<double> x)
        {
            return 1 / (1 + (-_modelParameters.GetModelParameter(Weights).Values * x -
                             _modelParameters.GetModelParameter(Bias).Values).PointwiseExp());
        }

        public Dictionary<string, Vector<double>> ComputeGradients(Matrix<double> x, Vector<double> y)
        {
            var prediction = Predict(x);
            return new Dictionary<string, Vector<double>>
            {
                [Bias] = (prediction - y) * x,
                [Weights] = Vector<double>.Build.DenseOfEnumerable(new List<double> {(prediction - y).Average()})
            };
        }

        public void UpdateParameter(string parameterName, Vector<double> values)
        {
            _modelParameters.UpdateModelParameter(parameterName, values);
        }

        public ModelParametersList GetParameters()
        {
            return _modelParameters;
        }

        public double ComputeLoss(Matrix<double> x, Vector<double> target)
        {
            var prediction = Predict(x);
            return -(target.PointwiseMultiply(prediction.PointwiseLog()) +
                     (1 - target).PointwiseMultiply((1 - prediction).PointwiseLog())).Average();
        }
    }
}