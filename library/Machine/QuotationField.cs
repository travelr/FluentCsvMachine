using FluentCsvMachine.Exceptions;
using System.Text;

namespace FluentCsvMachine.Machine
{
    /// <summary>
    /// Machine reading quoted CSV fields according RFC 4180
    /// https://en.wikipedia.org/wiki/Comma-separated_values
    /// </summary>
    internal class QuotationField<T> where T : new()
    {
        internal enum States
        {
            Initial,
            Running, // Quote Open
            Closed,  // Second Quote
            //Initial -> Closed and Delimiter or Linebreak
        }

        internal States State { get; private set; }

        public CsvConfiguration Config { get; }

        private readonly StringBuilder sb = new();
        private readonly Line<T> line;

        public QuotationField(Line<T> lineMachine)
        {
            line = lineMachine;
            Config = line.Config;
        }

        /// <summary>
        /// Process the current char in the stream
        /// </summary>
        /// <param name="c">current char</param>
        public void Process(char c)
        {
            switch (c, State)
            {
                case var t when (t.State == States.Initial && t.c == Config.Quote):
                    // First quote
                    State = States.Running;
                    break;

                case var t when (t.State == States.Running && t.c != Config.Quote):
                    // Quote content
                    sb.Append(c);
                    break;

                case var t when (t.State == States.Running && t.c == Config.Quote):
                    // Second quote
                    State = States.Closed;
                    break;

                case var t when (t.State == States.Closed && (t.c == Config.Delimiter || t.c == Config.NewLine)):
                    // Second quote followed by a delimiter or linebreak
                    var value = sb.ToString();
                    sb.Clear();
                    line.Value(value);
                    State = States.Initial;
                    break;

                case var t when (t.State == States.Closed && t.c == Config.Quote):
                    // Quote inside a quoted field ""asd"" -> "asd"
                    sb.Append(Config.Quote);
                    State = States.Running;
                    break;

                default:
                    throw new CsvMachineException();
            }
        }
    }
}