using System.Numerics;

namespace FluentCsvMachine.Machine.Values
{
    /// <summary>
    /// Parser for floating point numbers
    /// </summary>
    /// <typeparam name="T">Type e.g. decimal</typeparam>
    internal class FloatingPointParser<T> : ValueParser where T : IFloatingPoint<T>
    {
        private bool _isNull;
        private long _n;
        private int? _s; // index of the floating point char
        private int _charCount; // length of the field

        internal override void Process(char c)
        {
            // Input check
            var isFloatingPointChar = c == Config!.FloatingPoint;
            if ((c < '0' || c > '9') && !isFloatingPointChar)
            {
                _isNull = true;
                State = States.FastForward;
                return;
            }

            // Update internal variables
            if (isFloatingPointChar)
            {
                _s = _charCount + 1;
            }
            else
            {
                _n = (_n * 10) + (c - '0');
            }

            _charCount++;
        }

        internal override ResultValue? GetResult()
        {
            ResultValue? result = null;

            // Create result if it is a valid field
            if (!_isNull)
            {
                // Based on https://stackoverflow.com/a/8458496
                var resultValue = new decimal((int)_n, (int)(_n >> 32), 0, false, (byte)(_charCount - (_s ?? _charCount)));
                // Cast will happen later
                result = new ResultValue(typeof(T), resultValue);
            }

            // Reset variables
            _isNull = false;
            _n = 0;
            _s = null;
            _charCount = 0;
            State = States.Parsing;

            return result;
        }
    }
}