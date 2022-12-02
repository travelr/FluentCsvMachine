using FluentCsvMachine.Exceptions;
using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Result;
using FluentCsvMachine.Machine.States;
using FluentCsvMachine.Machine.Values;
using FluentCsvMachine.Machine.Workflow;
using FluentCsvMachine.Property;
using System.Runtime.CompilerServices;

namespace FluentCsvMachine.Machine
{
    internal class CsvMachine<T> where T : new()
    {
        internal enum States
        {
            HeaderSearch,
            Content
        }

        internal CsvConfiguration Config { get; }

        internal States State { get; private set; }

        private readonly List<CsvPropertyBase> properties;
        private readonly List<Action<T, IReadOnlyList<object?>>>? lineActions;
        private readonly char skipNewLineChar;
        private readonly Line<T> lineMachine;
        private readonly StringParser stringParser = new();
        private readonly Action<ResultLine> insertQueue;


        private readonly List<CsvPropertyBase> _properties;
        private readonly List<Action<T, IReadOnlyList<object?>>>? _lineActions;

        private readonly Line<T> _line;
        private readonly List<T> result;

        private readonly char _skipNewLineChar;
        private ValueParser?[]? _parsers;
        private readonly StringParser _stringParser = new();


        /// <summary>
        /// Factory for entities
        /// Assign only after headers are found because the Index value is required
        /// </summary>
        internal EntityFactory<T>? Factory { get; private set; }

        public CsvMachine(WorkflowInput<T> input, Action<ResultLine> insertQueue)
        {
            Guard.IsNotNull(input);
            Guard.IsNotNull(insertQueue);

            Config = input.Config;
            State = States.HeaderSearch;

            properties = input.Properties;
            lineActions = input.LineActions;
            skipNewLineChar = Config.NewLine == '\n' ? '\r' : '\0';
            lineMachine = new Line<T>(this);
            this.insertQueue = insertQueue;
        }

        internal void Process(char[] buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (buffer[i] == skipNewLineChar)
                {
                    continue;
                }

                lineMachine.Process(buffer[i]);
            }
        }

        /// <summary>
        /// A full line was parsed, here is the result
        /// </summary>
        /// <param name="line">The line, empty strings are null</param>
        internal void ResultLine(ref ResultLine line)
        {
            switch (State)
            {
                case States.HeaderSearch:
                    // Header Search is only using string
                    FindAndSetHeaders(ref line);
                    break;

                case States.Content:
                    insertQueue(line);
                    break;

                default: throw new CsvMachineException("Unknown CsvMachine state");
            }
        }

        /// <summary>
        /// Process the final line if the file does not end with a line break
        /// </summary>
        internal void EndOfFile()
        {
            if (lineMachine.State != 0)
            {
                lineMachine.Process(Config.NewLine);
            }
        }

        /// <summary>
        /// Sets the state machine to Content
        /// Precondition: Set the CSV index for the Properties
        /// </summary>
        internal void SetStateContent()
        {
            if (!properties.Any(x => x.Index.HasValue))
            {
                throw new CsvMachineException("Property Index needs to be set first, before calling this method");
            }

            // Generate Factory
            Factory = new EntityFactory<T>(properties, lineActions);

            // Focus on content now
            State = States.Content;


            // Generate parser array
            var validProps = _properties.Where(x => x.Index.HasValue).ToArray();
            var maxCols = validProps.Max(x => x.Index)!.Value + 1;
            _parsers = new ValueParser[maxCols];
            foreach (var prop in validProps)

            {
                prop.ValueParser!.Config = Config;
                _parsers[prop.Index!.Value] = prop.ValueParser;
            }

            // Change the parser
            lineMachine.SetParserAndResetState();
        }

        /// <summary>
        /// Get the appropriate parser or default to the string parser
        /// </summary>
        /// <param name="columnNumber">Current column number in the CSV line</param>
        /// <returns>The correct parser</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueParser? GetParser(int columnNumber)
        {
            return State switch
            {
                States.HeaderSearch => _stringParser,
                States.Content => columnNumber < _parsers!.Length ? _parsers![columnNumber]! : null,
                _ => throw new NotImplementedException()
            };
        }

        #region Private

        /// <summary>
        /// Find Header Line
        /// </summary>
        private void FindAndSetHeaders(ref ResultLine line)
        {
            if (lineMachine.LineCounter >= Config.HeaderSearchLimit)
            {
                ThrowHelper.ThrowCsvMalformedException(
                    "Header not found in CSV file, please check your delimiter or the column definition!");
            }


            var headers = _properties.Select(x => x.ColumnName!);
            var fields = new List<string?>();
            foreach (ref readonly var x in line.AsSpan())
            {
                fields.Add(x.IsNull ? null : x.Value as string);
            }

            // Are all headers present in this line?
            if (!headers.All(x => fields.Any(y => y != null && x == y)))
            {
                return;
            }

            // All header Strings are included in the CSV line
            // Create structure so the CSV index to the properties
            var i = 0;
            var headersDic = new Dictionary<string, int>();
            foreach (var header in fields)
            {
                if (header != null)
                {
                    headersDic[header] = i;
                }

                i++;
            }

            // Set CSV row index for all properties
            foreach (var p in properties)
            {
                p.Index = headersDic[p.ColumnName!];
            }

            SetStateContent();
        }

        #endregion Private
    }
}