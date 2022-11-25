using FluentCsvMachine.Machine.Result;

namespace FluentCsvMachine.Machine.Values
{
    internal class CharParser : ValueParser
    {
        private char? r;

        public CharParser(bool nullable) : base(nullable)
        {
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
                var returnValue = new ResultValue(typeof(char), r);
                r = null;
                return returnValue;
            }
        }
    }
}