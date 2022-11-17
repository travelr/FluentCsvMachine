using FluentCsvMachine.Helpers;

namespace FluentCsvMachine.Machine.Values
{
    /// <summary>
    /// DateTime parser
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

        public DateTimeParser(string inputFormat)
        {
            Guard.IsNotNullOrEmpty(inputFormat);

            _inputFormat = inputFormat;
        }

        internal override void Process(char c)
        {
            if (_charCount >= _inputFormat.Length)
            {
                ThrowHelper.ThrowCsvMalformedException($"Cannot parse DateTime. Column is longer than the input format ({_inputFormat}");
            }

            switch (_inputFormat[_charCount])
            {
                case 'y':
                    _year = _year * 10 + (c - '0');
                    break;

                case 'M':
                    _month = _month * 10 + (c - '0');
                    break;

                case 'd':
                    _day = _day * 10 + (c - '0');
                    break;

                case 'T':
                    if (c == 'p' || c == 'P')
                        _hourOffset = 12;
                    break;

                case 'h':
                    _hour = _hour * 10 + (c - '0');
                    if (_hour == 12) _hour = 0;
                    break;

                case 'H':

                    _hour = _hour * 10 + (c - '0');
                    _hourOffset = 0;
                    break;

                case 'm':
                    _minute = _minute * 10 + (c - '0');
                    break;

                case 's':
                    _second = _second * 10 + (c - '0');
                    break;

                case 'f':
                    _ms = _ms * 10 + (c - '0');
                    break;
            }

            _charCount++;
        }

        internal override ResultValue GetResult()
        {
            DateTime? resultValue = null;
            try
            {
                if (_year < 99)
                {
                    _year = _year > 35 ? _year + 1900 : _year + 2000;
                }
                resultValue = new DateTime(_year, _month, _day, _hour + _hourOffset, _minute, _second, _ms);
            }
            catch (Exception)
            {
                ThrowHelper.ThrowCsvMalformedException($"Cannot parse DateTime. Error In Date: {_year}/{_month}/{_day} {_hour + _hourOffset}:{_minute}:{_second}.{_ms}");
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

            return new ResultValue(typeof(DateTime), resultValue);
        }
    }
}