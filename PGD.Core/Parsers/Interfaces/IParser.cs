using MathNet.Numerics.LinearAlgebra;

namespace PGD.Core.Parsers.Interfaces
{
    public interface IParser
    {
        public (Matrix<double>, Vector<double>) Parse(string fileName);
    }
}