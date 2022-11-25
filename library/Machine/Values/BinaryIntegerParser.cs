using System.Numerics;
using FluentCsvMachine.Machine.Result;

namespace FluentCsvMachine.Machine.Values
{
    //ToDo: Support unsigned numbers

    /// <summary>
    /// Parser for short, int, long, ...
    /// </summary>
    /// <typeparam name="T">IBinaryInteger</typeparam>
    internal class BinaryIntegerParser<T> : ValueParser where T : IBinaryInteger<T>
    {
        private bool _isNull;
        private T _result = T.CreateChecked(0);

        // Look up table, X == 10
        private readonly Dictionary<char, T> _convert = new()
        {
            { '0', T.CreateChecked(0) },
            { '1', T.CreateChecked(1) },
            { '2', T.CreateChecked(2) },
            { '3', T.CreateChecked(3) },
            { '4', T.CreateChecked(4) },
            { '5', T.CreateChecked(5) },
            { '6', T.CreateChecked(6) },
            { '7', T.CreateChecked(7) },
            { '8', T.CreateChecked(8) },
            { '9', T.CreateChecked(9) },
            { 'X', T.CreateChecked(10) }
        };

        private bool _isNegative;

        internal override void Process(char c)
        {
            if (_result == _convert['0'] && c == '-')
            {
                _isNegative = true;
                return;
            }

            // Input check
            if (c is not (>= '0' and <= '9'))
            {
                _isNull = true;
                State = States.FastForward;
                return;
            }

            // Calculate new result
            _result = _convert['X'] * _result + _convert[c];
        }

        internal override ResultValue GetResult()
        {
            if (_isNegative)
            {
                _result *= T.CreateChecked(-1);
            }

            var result = !_isNull ? new ResultValue(typeof(T), _result) : new ResultValue();

            // Reset variables
            _result = T.CreateChecked(0);
            _isNull = false;
            _isNegative = false;
            State = States.Parsing;

            return result;
        }
    }
}