using System.Text;

namespace FluentCsvMachine.Machine
{
    /// <summary>
    /// Machine reading quoted CSV fields according RFC 4180
    /// https://en.wikipedia.org/wiki/Comma-separated_values
    /// </summary>
    internal class QuotationField
    {
        internal enum States
        {
            Initial,
            Running, // Quote Open
            Closed,  // Second Quote
            //Initial -> Closed and Delimiter or Linebreak
        }

        internal States State { get; private set; }

        private readonly StringBuilder sb = new();
        private readonly Line line;

        public QuotationField(Line lineMachine)
        {
            line = lineMachine;
        }

        /// <summary>
        /// Process the current char in the stream
        /// </summary>
        /// <param name="c">current char</param>
        public void Process(char c)
        {
            switch (c, State)
            {
                case var t when (t.State == States.Initial && t.c == '"'):
                    // First quote
                    State = States.Running;
                    break;

                case var t when (t.State == States.Running && t.c != '"'):
                    // Quote content
                    sb.Append(c);
                    break;

                case var t when (t.State == States.Running && t.c == '"'):
                    // Second quote
                    State = States.Closed;
                    break;

                case var t when (t.State == States.Closed && (t.c == ',' || t.c == '\n')):
                    // Second quote followed by a delimiter or linebreak
                    var value = sb.ToString();
                    sb.Clear();
                    line.Value(value);
                    State = States.Initial;
                    break;

                case var t when (t.State == States.Closed && t.c == '"'):
                    // Quote inside a quoted field ""asd"" -> "asd"
                    sb.Append('"');
                    State = States.Running;
                    break;

                default:
                    throw new Exception("Unkown state");
            }
        }
    }
}