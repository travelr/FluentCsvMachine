using System.Text;

namespace FluentCsvMachine.Machine.Values
{
    internal class StringParser : ValueParser
    {
        private readonly StringBuilder _sb = new(25);

        internal override void Process(char c)
        {
            _sb.Append(c);
        }

        internal override ResultValue? GetResult()
        {
            var returnValue = _sb.Length > 0 ? new ResultValue(typeof(string), _sb.ToString()) : null;
            _sb.Clear();
            return returnValue;
        }
    }
}