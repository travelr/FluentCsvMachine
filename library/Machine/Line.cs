using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCsvMachine.Machine
{
    internal class Line
    {
        internal enum States
        {
            Initial,
            Field,
            FieldQuoted,
            Comment
        }

        private readonly CsvMachine csv;
        private readonly Field field;
        private readonly QuotationField quote;

        // Fields of the current line
        private readonly List<string?> fields;

        // Column number in this line
        private int colCount;

        internal States State { get; private set; }

        public Line(CsvMachine csv)
        {
            State = States.Initial;
            this.csv = csv;
            field = new Field(this);
            quote = new QuotationField(this);
            fields = new List<string?>(csv.headers.Length);
            colCount = 0;
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
                case var t when (t.State == States.Initial && (t.c != '"' && c != '\n' && c != '#')):
                    // Normal new line, first field without a quote
                    State = States.Field;
                    field.Process(c);
                    break;

                case var t when (t.State == States.Initial && t.c == '"'):
                    // Normal new line, first field with a quote
                    State = States.FieldQuoted;
                    quote.Process(c);
                    break;

                case var t when (t.State == States.Initial && t.c == '#'):
                    // Comment line, nothing to do
                    State = States.Comment;
                    return;

                case var t when (t.State == States.Initial && t.c == '\n'):
                    // Empty Line, nothing to do
                    LineCounter++;
                    return;

                case var t when (t.State == States.Field):
                    // Reading unquoted field
                    field.Process(c);
                    break;

                case var t when (t.State == States.FieldQuoted):
                    // Reading quoted field
                    quote.Process(c);
                    break;

                case var t when (t.State == States.Comment && t.c != '\n'):
                    // Reading comment field
                    return;

                default:
                    if (c != '\n')
                    {
                        // Only line breaks should remain
                        throw new Exception("Unkown state");
                    }
                    break;
            }

            // Nothing left to do if it is not a line break
            // Also allow line breaks in quoted fields
            // Line breaks also need to be processed by the sub state machine
            if (c != '\n' || State == States.FieldQuoted && quote.State != QuotationField.States.Initial)
            {
                return;
            }

            // Report line on CSV machine
            if (State != States.Comment)
            {
                csv.ResultLine(fields);
                fields.Clear();
                colCount = 0;
            }

            // Reset machine
            LineCounter++;
            State = States.Initial;
        }

        /// <summary>
        /// Sub state machine calls this method when a csv field has been fully read
        /// </summary>
        /// <param name="value">csv field value</param>
        /// <exception cref="Exception"></exception>
        internal void Value(string value)
        {
            if (colCount >= csv.headers.Length)
            {
                throw new Exception("File has more columns than defined");
            }

            fields.Add(value != string.Empty ? value : null);
            colCount++;
            State = States.Initial;
        }
    }
}