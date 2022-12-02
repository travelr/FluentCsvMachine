using FluentCsvMachine.Machine.Result;

namespace FluentCsvMachine.Machine.Values
{
    internal class CharParser : ValueParser
    {
        private object? r;
        private Type _resultType;

        public CharParser(bool nullable) : base(nullable)
        {
            _resultType = GetResultType<char>();
        }

        internal override void Process(char c)
        {
            r = c;
        }

        internal override ResultValue GetResult()
        {
            if (r == null)
            {
                SetNull(); // Throw an Exception if Nullable is not allowed
                return new ResultValue();
            }
            else
            {
                var returnValue = new ResultValue(ref _resultType, ref r);
                r = null;
                return returnValue;
            }
        }
    }
}