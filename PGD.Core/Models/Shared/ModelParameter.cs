using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;

namespace PGD.Core.Models.Shared
{
    public class ModelParameter
    {
        public string Name { get; set; }

        public Vector<double> Values { get; set; }

        public void Deconstruct(out string parameterName, out Vector<double> parameterValues)
        {
            parameterName = Name;
            parameterValues = Values;
        }
    }

    public class ModelParametersList : IEnumerable<ModelParameter>
    {
        private IEnumerable<ModelParameter> _modelParameters;

        public ModelParametersList(IEnumerable<ModelParameter> modelParameters)
        {
            _modelParameters = modelParameters;
        }

        public bool TryGetModelParameter(string name, out ModelParameter parameter)
        {
            parameter = _modelParameters.FirstOrDefault(x => x.Name == name);
            return parameter != null;
        }

        public void TryUpdateModelParameter(string name, Vector<double> values)
        {
            if (TryGetModelParameter(name, out var modelParameter))
                modelParameter.Values = values;
        }

        public ModelParameter GetModelParameter(string name)
        {
            if (TryGetModelParameter(name, out var parameter))
                return parameter;
            throw new ArgumentOutOfRangeException($"Model parameter with name '{name}' not found.");
        }

        public void CreateModelParameter(string name, Vector<double> values)
        {
            if (!TryGetModelParameter(name, out var parameter))
                _modelParameters = _modelParameters.Append(new ModelParameter
                {
                    Name = name,
                    Values = values
                });
            else
                throw new ArgumentException("Model parameter with name '{name}' is already present.");
        }

        public void UpdateModelParameter(string name, Vector<double> values)
        {
            if (TryGetModelParameter(name, out var modelParameter))
                modelParameter.Values = values;
            else
                throw new ArgumentOutOfRangeException($"Model parameter with name '{name}' not found.");
        }

        public IEnumerator<ModelParameter> GetEnumerator()
        {
            return _modelParameters.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _modelParameters.GetEnumerator();
        }
    }
}