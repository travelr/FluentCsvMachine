using FluentCsvMachine.Exceptions;

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
            //Initial -> Closed and Delimiter or line-break
        }

        internal States State { get; private set; }

        public CsvConfiguration Config { get; }

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
                case { State: States.Initial } t when t.c == Config.Quote:
                    // First quote
                    State = States.Running;
                    break;

                case { State: States.Running } t when t.c != Config.Quote:
                    // Quote content
                    line.Parser.Process(c);
                    break;

                case { State: States.Running } t when t.c == Config.Quote:
                    // Second quote
                    State = States.Closed;
                    break;

                case { State: States.Closed } t when (t.c == Config.Delimiter || t.c == Config.NewLine):
                    // Second quote followed by a delimiter or line break
                    line.Value();
                    State = States.Initial;
                    break;

                case { State: States.Closed } t when t.c == Config.Quote:
                    // Quote inside a quoted field ""hi"" -> "hi"
                    line.Parser.Process(Config.Quote);
                    State = States.Running;
                    break;

                default:
                    throw new CsvMachineException();
            }
        }
    }
}