using System.Text;

namespace FluentCsvMachine.Machine
{
    /// <summary>
    /// Machine reading unquoted CSV fields
    /// </summary>
    internal class Field<T> where T : new()
    {
        private readonly StringBuilder sb = new();
        private readonly Line<T> line;

        public CsvConfiguration Config { get; }

        public Field(Line<T> lineMachine)
        {
            line = lineMachine;
            Config = line.Config;
        }

        public void Process(char c)
        {
            if (c == Config.Delimiter || c == Config.NewLine)
            {
                // End field on the delimiter or line break
                var value = sb.ToString();
                sb.Clear();
                line.Value(value);
            }
            else
            {
                // Allowed char
                sb.Append(c);
            }
        }
    }
}