using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using PGD.Core.Models.Shared;

namespace PGD.Core.Models.Interfaces
{
    public interface IModel
    {
        public Vector<double> Predict(Matrix<double> x);
        public Dictionary<string, Vector<double>> ComputeGradients(Matrix<double> x, Vector<double> y);
        public double ComputeLoss(Matrix<double> x, Vector<double> y);
        public void UpdateParameter(string parameterName, Vector<double> values);
        public ModelParameter GetParameter(string parameterName);
        public ModelParametersList GetParameters();
        public void Initialize(int dimensionsCount);
    }
}