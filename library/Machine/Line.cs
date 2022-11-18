using FluentCsvMachine.Exceptions;
using FluentCsvMachine.Machine.Values;

namespace FluentCsvMachine.Machine
{
    internal class Line<T> : CsvBaseElement where T : new()
    {
        internal enum States
        {
            // New field
            Initial,

            Field,
            FieldQuoted,
            Comment,
            Skip // Column, because it is not mapped
        }

        private readonly CsvMachine<T> csv;
        private readonly Field<T> field;
        private readonly QuotationField<T> quote;

        // Fields of the current line
        private readonly List<ResultValue> fields;

        // Column number in this line
        private int columnNumber;


        internal States State { get; private set; }

        internal ValueParser Parser { get; private set; }

        public Line(CsvMachine<T> csv) : base(csv.Config)
        {
            this.csv = csv;
            State = States.Initial;

            Parser = new StringParser();

            field = new Field<T>(this, csv.Config);
            quote = new QuotationField<T>(this, csv.Config);
            fields = new List<ResultValue>(20); //ToDO: FIx


            columnNumber = 0;
        }

        public int LineCounter { get; private set; }

        /// <summary>
        /// Process the current char in the stream
        /// </summary>
        /// <param name="c">current char</param>
        public void Process(char c)
        {
            // Always process the sub state machine before continuing with this one
            switch (c, State)
            {
                case { State: States.Initial } t when (t.c != Quote && c != NewLine && c != Comment):
                    // New field without a quote
                    State = States.Field;

                    field.Process(c);
                    break;

                case { State: States.Initial } t when t.c == Quote:
                    // New field with a quote
                    State = States.FieldQuoted;

                    quote.Process(c);
                    break;

                case { State: States.Initial } t when Comment.HasValue && t.c == Comment:
                    // Comment line, nothing to do
                    State = States.Comment;
                    return;

                case { State: States.Initial } t when t.c == NewLine:
                    // Empty Line, nothing to do
                    LineCounter++;
                    return;

                case { State: States.Field }:
                    // Reading unquoted field
                    field.Process(c);
                    break;

                case { State: States.FieldQuoted }:
                    // Reading quoted field
                    quote.Process(c);
                    break;

                case { State: States.Comment } t when t.c != NewLine:
                    // Reading comment field
                    return;

                case { State: States.Skip } t when t.c != Delimiter:
                    // Char of skip column
                    return;

                case { State: States.Skip } t when t.c == Delimiter:
                    columnNumber++;
                    SetParserAndState();
                    return;

                default:
                    if (c != NewLine)
                    {
                        // Only line breaks should remain
                        throw new CsvMachineException();
                    }

                    break;
            }

            // Nothing left to do if it is not a line break
            // Also allow line breaks in quoted fields
            // Line breaks also need to be processed by the sub state machine
            if (c != NewLine || (State == States.FieldQuoted && quote.State != QuotationField<T>.States.Initial))
            {
                return;
            }

            // Report line on CSV machine
            if (State != States.Comment)
            {
                csv.ResultLine(fields);
                fields.Clear();
                columnNumber = 0;
            }

            // Reset machine
            LineCounter++;
            SetParserAndState();
        }

        /// <summary>
        /// Sub state machine calls this method when a csv field has been fully read
        /// </summary>
        /// <exception cref="Exception"></exception>
        internal void Value()
        {
            var value = Parser.GetResult();
            fields.Add(value);
            columnNumber++;
            SetParserAndState();
        }

        private void SetParserAndState()
        {
            if (csv.State == CsvMachine<T>.States.Content)
            {
                Parser = csv.GetParser(columnNumber);
                //ToDo: Always true
                State = Parser != null ? States.Initial : States.Skip;
            }
            else
            {
                // Header Search
                State = States.Initial;
            }
        }
    }
}