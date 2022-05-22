using MathNet.Numerics.Data.Text;
using MathNet.Numerics.LinearAlgebra;
using Microsoft.Extensions.Options;
using PGD.Core.Parsers.Interfaces;

namespace PGD.Core.Parsers.Implementation.Csv
{
    public class CsvParser : IParser
    {
        private readonly IOptions<CsvParserOptions> _csvOptions;

        public CsvParser(IOptions<CsvParserOptions> csvOptions)
        {
            _csvOptions = csvOptions;
        }

        public (Matrix<double>, Vector<double>) Parse(string fileName)
        {
            var matrix = DelimitedReader.Read<double>(
                fileName, false,
                _csvOptions.Value.Delimiter, _csvOptions.Value.HasHeaders);
            var y = matrix.Column(matrix.ColumnCount - 1);
            var x = matrix.RemoveColumn(matrix.ColumnCount - 1);
            return (x, y);
        }
    }
}