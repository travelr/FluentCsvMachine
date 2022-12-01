using FluentCsvMachine.Helpers;
using System.Runtime.CompilerServices;
using FluentCsvMachine.Machine.Result;

namespace FluentCsvMachine.Machine.Values
{
    /// <summary>
    /// DateTime parser
    /// Input format requires to be a even number of input chars 
    /// Roughly based on https://stackoverflow.com/a/16920747
    /// </summary>
    internal class DateTimeParser : ValueParser
    {
        private readonly string _inputFormat;

        private int _charCount;
        private int _year;
        private int _month;
        private int _day;
        private int _hour;
        private int _minute;
        private int _second;
        private int _hourOffset;
        private int _ms;

        private int _inputCount;
        private char? _lastInputChar;
        private Type _resultType;

        public DateTimeParser(string inputFormat, bool nullable) : base(nullable)
        {
            Guard.IsNotNullOrEmpty(inputFormat);

            _inputFormat = inputFormat;
            _resultType = !nullable ? typeof(DateTime) : typeof(DateTime?);
        }

        internal override void Process(char c)
        {
            if (_charCount >= _inputFormat.Length)
            {
                ThrowHelper.ThrowCsvMalformedException($"Cannot parse DateTime. Column is longer than the input format ({_inputFormat}");
            }

            var inputChar = _inputFormat[_charCount];

            if (c is >= '0' and <= '9' || (inputChar is 'T' && c is 'p' or 'P'))
            {
                // Is a number
                var value = (c - '0');

                switch (inputChar)
                {
                    case 'y':
                        _year = _year * 10 + value;
                        CountInput('y');
                        break;

                    case 'M':
                        _month = _month * 10 + value;
                        CountInput('M');
                        break;

                    case 'd':
                        _day = _day * 10 + value;
                        CountInput('d');
                        break;

                    case 'T':
                        if (c is 'p' or 'P')
                            _hourOffset = 12;
                        break;

                    case 'h':
                        _hour = _hour * 10 + value;
                        if (_hour == 12) _hour = 0;
                        CountInput('h');
                        break;

                    case 'H':

                        _hour = _hour * 10 + value;
                        _hourOffset = 0;
                        CountInput('H');
                        break;

                    case 'm':
                        _minute = _minute * 10 + value;
                        CountInput('m');
                        break;

                    case 's':
                        _second = _second * 10 + value;
                        CountInput('s');
                        break;

                    case 'f':
                        _ms = _ms * 10 + value;
                        CountInput('f');
                        break;
                }

                // Remember last input char so that you can count _inputCount
                _lastInputChar = inputChar;
            }
            else
            {
                // Not a number

                // Assumes that all input formats have a number of two place holders
                if (_inputCount % 2 > 0)
                {
                    // If the input does not match that skip a char in the input value
                    _charCount++;
                }

                _inputCount = 0;
                _lastInputChar = null;
            }


            _charCount++;
        }

        internal override ResultValue GetResult()
        {
            var resultValue = new ResultValue();

            if (_year > 0)
            {
                try
                {
                    object dt = new DateTime(_year, _month, _day, _hour + _hourOffset, _minute, _second, _ms);
                    resultValue = new ResultValue(ref _resultType, ref dt);
                }
                catch (ArgumentOutOfRangeException)
                {
                    if (!Nullable)
                    {
                        ThrowHelper.ThrowCsvMalformedException(
                            $"Cannot parse DateTime. Error In Date: {_year}/{_month}/{_day} {_hour + _hourOffset}:{_minute}:{_second}.{_ms}");
                    }
                }
            }

            // Reset values
            _charCount = 0;
            _year = 0;
            _month = 0;
            _day = 0;
            _hour = 0;
            _minute = 0;
            _second = 0;
            _hourOffset = 0;
            _ms = 0;
            _inputCount = 0;
            _lastInputChar = null;

            return resultValue;
        }


        /// <summary>
        /// Counts the number of input chars (e.g. (dd.MM.yyyy) where d M Y are all input chars)
        /// </summary>
        /// <param name="inputChar">what char to compare to</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CountInput(char inputChar)
        {
            if (_lastInputChar == null || _lastInputChar == inputChar)
            {
                _inputCount++;
            }
        }
    }
}