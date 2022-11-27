using FluentCsvMachine.Machine.Result;

namespace FluentCsvMachine.Machine.Values
{
    internal class CharParser : ValueParser
    {
        private char? r;
        private readonly Type _resultType;

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
                var returnValue = new ResultValue(_resultType, r);
                r = null;
                return returnValue;
            }
        }
    }
}