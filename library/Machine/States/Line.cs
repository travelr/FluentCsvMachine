using FluentCsvMachine.Exceptions;
using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Result;
using FluentCsvMachine.Machine.Values;
using System.Runtime.CompilerServices;

namespace FluentCsvMachine.Machine.States
{
    internal class Line<T> : BaseElement where T : new()
    {
        internal enum States
        {
            // New field
            Initial = 0,
            Field,
            FieldQuoted,
            Comment
        }

        private readonly CsvMachine<T> csv;
        private readonly Field<T> field;
        private readonly QuotationField<T> quote;

        // Fields of the current line
        private ResultValue[] fields;

        // Column number in this line
        private int _fieldsIndex;
        private readonly int maxNumberOfColumns;


        internal States State
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get;
            private set;
        }

        internal ValueParser? Parser { get; private set; }

        public Line(CsvMachine<T> csv) : base(csv.Config)
        {
            this.csv = csv;
            State = States.Initial;

            Parser = new StringParser();

            field = new Field<T>(this, csv.Config);
            quote = new QuotationField<T>(this, csv.Config);


            maxNumberOfColumns = csv.Config.MaxNumberOfColumns;
            fields = new ResultValue[maxNumberOfColumns];
            _fieldsIndex = 0;
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
                case { State: States.Initial } t when t.c != Quote && c != NewLine && c != Comment:
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
                    if (_fieldsIndex == 0)
                    {
                        // Empty Line, nothing to do
                        LineCounter++;
                        return;
                    }

                    // else Delimiter Line Break 
                    break;

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


                default:
                    if (c != NewLine)
                    {
                        // Only line breaks should remain
                        throw new CsvMachineException($"Unknown Line state c == '{c}'");
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

                var line = new ResultLine(fields, _fieldsIndex, LineCounter);
                
                csv.ResultLine(ref line);
                _fieldsIndex = 0;
            }

            // Reset machine
            LineCounter++;
            SetParserAndResetState();
        }

        /// <summary>
        /// Sub state machine calls this method when a csv field has been fully read
        /// </summary>
        /// <exception cref="Exception"></exception>
        internal void Value()
        {
            if (_fieldsIndex >= maxNumberOfColumns)
            {
                ThrowHelper.ThrowCsvConfigurationException(
                    $"Please check the MaxNumberOfColumns option in the Configuration. We are exceeding {maxNumberOfColumns} columns");
            }

            var value = Parser?.GetResult() ?? new ResultValue();
            fields[_fieldsIndex++] = value;
            SetParserAndResetState();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void SetParserAndResetState()
        {
            State = States.Initial;
            if (csv.State == CsvMachine<T>.States.Content)
            {
                Parser = csv.GetParser(_fieldsIndex);
            }
        }
    }
}