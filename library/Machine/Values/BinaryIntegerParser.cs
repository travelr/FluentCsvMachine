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
            _resultType = GetResultType<T>();

            _convert = new T[12];
            for (int i = 0; i <= 10; i++)
            {
                _convert[i] = T.CreateChecked(i);
            }

            if (signed)
            {
                _convert[11] = T.CreateChecked(-1);
            }
        }

        private readonly bool _signed;
        private readonly T[] _convert;

        private bool _isNegative;
        private T _result;
        private bool _resultAssigned;
        private readonly Type _resultType;

        internal override void Process(char c)
        {
            if (_signed && _result == _convert[0] && c == '-')
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
            _result = _convert[10] * _result + _convert[c - '0'];
            _resultAssigned = true;
        }


        internal override ResultValue GetResult()
        {
            ResultValue result;
            if (!IsNull && _resultAssigned)
            {
                if (_signed && _isNegative)
                {
                    // 11 == -1
                    _result *= _convert[11];
                }


                result = new ResultValue(_resultType, _result);
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