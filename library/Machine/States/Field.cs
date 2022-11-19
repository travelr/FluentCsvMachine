using ParserStates = FluentCsvMachine.Machine.Values.ValueParser.States;

namespace FluentCsvMachine.Machine.States
{
    /// <summary>
    /// Machine reading unquoted CSV fields
    /// </summary>
    internal class Field<T> : BaseElement where T : new()
    {
        private readonly Line<T> _line;

        public Field(Line<T> line, CsvConfiguration config) : base(config)
        {
            _line = line;
        }

        public void Process(char c)
        {
            if (c == Delimiter || c == NewLine)
            {
                // End field on the delimiter or line break
                _line.Value();
            }
            else if (_line.Parser.State != ParserStates.FastForward)
            {
                // Allowed char
                _line.Parser.Process(c);
            }
        }
    }
}