using System.Numerics;

namespace FluentCsvMachine.Machine.Values
{
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
            {'0',  T.CreateChecked(0)},
            {'1',  T.CreateChecked(1)},
            {'2',  T.CreateChecked(2)},
            {'3',  T.CreateChecked(3)},
            {'4',  T.CreateChecked(4)},
            {'5',  T.CreateChecked(5)},
            {'6',  T.CreateChecked(6)},
            {'7',  T.CreateChecked(7)},
            {'8',  T.CreateChecked(8)},
            {'9',  T.CreateChecked(9)},
            {'X',  T.CreateChecked(10)},
        };

        internal override void Process(char c)
        {
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
            var result = !_isNull ? new ResultValue(typeof(T), _result) : new ResultValue();

            // Reset variables
            _result = T.CreateChecked(0);
            _isNull = false;
            State = States.Parsing;

            return result;
        }
    }
}