using FluentCsvMachine.Exceptions;

namespace FluentCsvMachine.Machine
{
    /// <summary>
    /// Machine reading quoted CSV fields according RFC 4180
    /// https://en.wikipedia.org/wiki/Comma-separated_values
    /// </summary>
    internal class QuotationField<T> : CsvBaseElement where T : new()
    {
        internal enum States
        {
            Initial,
            Running, // Quote Open
            Closed,  // Second Quote
            //Initial -> Closed and Delimiter or line-break
        }

        internal States State { get; private set; }


        private readonly Line<T> line;


        public QuotationField(Line<T> lineMachine, CsvConfiguration config) : base(config)
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
                case { State: States.Initial } t when t.c == Quote:
                    // First quote
                    State = States.Running;
                    break;

                case { State: States.Running } t when t.c != Quote:
                    // Quote content
                    line.Parser.Process(c);
                    break;

                case { State: States.Running } t when t.c == Quote:
                    // Second quote
                    State = States.Closed;
                    break;

                case { State: States.Closed } t when (t.c == Delimiter || t.c == NewLine):
                    // Second quote followed by a delimiter or line break
                    line.Value();
                    State = States.Initial;
                    break;

                case { State: States.Closed } t when t.c == Quote:
                    // Quote inside a quoted field ""hi"" -> "hi"
                    line.Parser.Process(Quote);
                    State = States.Running;
                    break;

                default:
                    throw new CsvMachineException($"Unknown Quotation state c == '{c}'");
            }
        }
    }
}