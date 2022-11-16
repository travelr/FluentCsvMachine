using ParserStates = FluentCsvMachine.Machine.Values.ValueParser.States;

namespace FluentCsvMachine.Machine
{
    /// <summary>
    /// Machine reading unquoted CSV fields
    /// </summary>
    internal class Field<T> where T : new()
    {
        private readonly Line<T> _line;

        public CsvConfiguration Config { get; }

        public Field(Line<T> lineMachine)
        {
            _line = lineMachine;
            Config = _line.Config;
        }

        public void Process(char c)
        {
            if (c == Config.Delimiter || c == Config.NewLine)
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