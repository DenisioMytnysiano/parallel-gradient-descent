using System;
using System.Collections.Generic;
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

            var chunkSize = (int) Math.Ceiling((double) chunkCount / target.Count);
            var chunks = new List<(Matrix<double> X, Vector<double> Y)>();
            for (var i = 0; i < chunkCount; i++)
            {
                var start = i * chunkSize;
                var x = input.SubMatrix(start, chunkCount, 0, input.ColumnCount);
                var y = target.SubVector(start, chunkSize);
                chunks.Add((x, y));
            }

            return chunks;
        }
    }
}