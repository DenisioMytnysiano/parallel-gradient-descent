using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace PGD.Core.Utils
{
    public static class DataUtils
    {
        public static IEnumerable<(Matrix<double> X, Vector<double> Y)> GetChunks(Matrix<double> input,
            Vector<double> target, int chunkCount)
        {
            if (input.RowCount != target.Count)
                throw new ArgumentException("There should be equal number of X and Y samples.");

            var chunkSize = (int) Math.Ceiling((double) target.Count / chunkCount);
            var chunks = new List<(Matrix<double> X, Vector<double> Y)>();
            for (var i = 0; i < chunkCount; i++)
            {
                var start = i * chunkSize;
                if (!(start >= input.RowCount))
                {
                    var x = input.SubMatrix(start, Math.Min(chunkSize, target.Count - start), 0, input.ColumnCount);
                    var y = target.SubVector(start, Math.Min(chunkSize, target.Count - start));
                    chunks.Add((x, y));
                }
            }

            return chunks;
        }

        public static (Matrix<double>, Vector<double>) GetRandomInput(int count, int dimensions, int seed)
        {
            var random = new Random(seed);
            var x = Matrix<double>.Build.Random(count, dimensions, seed);
            var y = Vector<double>.Build.DenseOfEnumerable(Enumerable.Range(0, count)
                .Select(i => (double) random.Next(0, 2)));
            return (x, y);
        }
    }
}