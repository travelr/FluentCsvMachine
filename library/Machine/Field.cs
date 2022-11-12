using System.Text;

namespace FluentCsvMachine.Machine
{
    /// <summary>
    /// Machine reading unquoted CSV fields
    /// </summary>
    internal class Field
    {
        private readonly StringBuilder sb = new();
        private readonly Line line;

        public Field(Line lineMachine)
        {
            line = lineMachine;
        }

        public void Process(char b)
        {
            if (b == ',' || b == '\n')
            {
                // End field on the delimiter or line break
                var value = sb.ToString();
                sb.Clear();
                line.Value(value);
            }
            else
            {
                // Allowed char
                sb.Append(b);
            }
        }
    }
}