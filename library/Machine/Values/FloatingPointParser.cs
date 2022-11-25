using System;
using System.Numerics;
using FluentCsvMachine.Machine.Result;

namespace FluentCsvMachine.Machine.Values
{
    /// <summary>
    /// Parser for floating point numbers
    /// </summary>
    /// <typeparam name="T">Type e.g. decimal</typeparam>
    internal class FloatingPointParser<T> : ValueParser where T : IFloatingPoint<T>
    {
        private bool _isNegative;
        private long? _n;
        private int? _s; // index of the floating point char
        private int _charCount; // length of the field

        public FloatingPointParser(bool nullable) : base(nullable)
        {
        }

        internal override void Process(char c)
        {
            // Input check
            if (_charCount == 0 && c == '-')
            {
                _isNegative = true;
                _charCount++;
                return;
            }

            var validChar = (c == Config!.DecimalPoint || c == Config!.ThousandsChar);
            if (c is < '0' or > '9' && !validChar)
            {
                SetNull();
                return;
            }

            // Update internal variables
            if (validChar)
            {
                _s = _charCount + 1;
            }
            else
            {
                _n = ((_n ?? 0) * 10) + (c - '0');
            }

            _charCount++;
        }

        internal override ResultValue GetResult()
        {
            ResultValue result;

            // Create result if it is a valid field
            if (!IsNull && _n.HasValue)
            {
                // Based on https://stackoverflow.com/a/8458496
                var resultValue = new decimal((int)_n, (int)(_n >> 32), 0, _isNegative, (byte)(_charCount - (_s ?? _charCount)));
                var resultType = Nullable ? typeof(Nullable<>).MakeGenericType(typeof(T)) : typeof(T);
                // Cast will happen later
                result = new ResultValue(resultType, resultValue);
            }
            else
            {
                result = new ResultValue();
            }

            // Reset variables
            _isNegative = false;
            _n = null;
            _s = null;
            _charCount = 0;
            IsNull = false;
            State = States.Parsing;

            return result;
        }
    }
}