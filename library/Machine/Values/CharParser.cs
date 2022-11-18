namespace FluentCsvMachine.Machine.Values
{
    internal class CharParser : ValueParser
    {
        private char? r;

        internal override void Process(char c)
        {
            r = c;
        }

        internal override ResultValue? GetResult()
        {
            var returnValue = r != null ? new ResultValue(typeof(char), r) : null;
            r = null;
            return returnValue;
        }
    }
}