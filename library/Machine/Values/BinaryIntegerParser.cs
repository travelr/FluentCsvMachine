using FluentCsvMachine.Machine.Result;
using System.Numerics;

namespace FluentCsvMachine.Machine.Values
{
    /// <summary>
    /// Parser for short, int, long, ...
    /// </summary>
    /// <typeparam name="T">IBinaryInteger</typeparam>
    internal class BinaryIntegerParser<T> : ValueParser where T : IBinaryInteger<T>
    {
        /// <summary>
        /// Parser for short, int, long, ..
        /// </summary>
        /// <param name="nullable">Type T is nullable</param>
        /// <param name="signed">Type T is a signed type</param>
        public BinaryIntegerParser(bool nullable, bool signed) : base(nullable)
        {
            _signed = signed;
            _result = T.CreateChecked(0);

            _convert = new Dictionary<char, T>
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

            if (signed)
            {
                _convert.Add('N', T.CreateChecked(-1));
            }
        }

        private readonly bool _signed;
        private readonly Dictionary<char, T> _convert;

        private bool _isNegative;
        private T _result;
        private bool _resultAssigned;

        internal override void Process(char c)
        {
            if (_signed && _result == _convert['0'] && c == '-')
            {
                _isNegative = true;
                return;
            }

            // Input check
            if (c is not (>= '0' and <= '9'))
            {
                SetNull();
                return;
            }

            // Calculate new result
            _result = _convert['X'] * _result + _convert[c];
            _resultAssigned = true;
        }


        internal override ResultValue GetResult()
        {
            ResultValue result;
            if (!IsNull && _resultAssigned)
            {
                if (_signed && _isNegative)
                {
                    _result *= _convert['N'];
                }

                var resultType = Nullable ? typeof(Nullable<>).MakeGenericType(typeof(T)) : typeof(T);
                result = new ResultValue(resultType, _result);
            }
            else
            {
                result = new ResultValue();
            }

            // Reset variables
            _result = T.CreateChecked(0);
            _resultAssigned = false;
            _isNegative = false;
            IsNull = false;
            State = States.Parsing;

            return result;
        }
    }
}