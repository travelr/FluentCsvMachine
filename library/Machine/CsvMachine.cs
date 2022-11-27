using System.Runtime.CompilerServices;
using FluentCsvMachine.Exceptions;
using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Result;
using FluentCsvMachine.Machine.States;
using FluentCsvMachine.Machine.Values;
using FluentCsvMachine.Machine.Workflow;
using FluentCsvMachine.Property;
using System.Threading.Channels;

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


        private readonly List<CsvPropertyBase> _properties;
        private readonly List<Action<T, IReadOnlyList<object?>>>? _lineActions;

        private readonly Line<T> _line;
        private readonly List<T> result;

        private readonly char _skipNewLineChar;
        private Dictionary<int, ValueParser>? _parsers;
        private readonly StringParser _stringParser = new();

        /// <summary>
        /// Factory for entities
        /// Assign only after headers are found because the Index value is required
        /// </summary>
        private EntityFactory<T>? _factory;


        public CsvMachine(WorkflowInput<T> input)
        {
            Guard.IsNotNull(input);


            Config = input.Config ?? new CsvConfiguration();
            State = States.HeaderSearch;


            _properties = input.Properties;
            _lineActions = input.LineActions;

            _line = new Line<T>(this);
            result = new List<T>();

            _skipNewLineChar = Config.NewLine == '\n' ? '\r' : '\0';
        }

        internal void Process(char[] buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (buffer[i] == _skipNewLineChar)
                {
                    continue;
                }

                _line.Process(buffer[i]);
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
                    var strLine = line.Fields.Select(x => !x.IsNull ? (string)x.Value! : null);
                    FindAndSetHeaders(strLine);
                    break;

                case States.Content:
                    var entity = _factory!.Create(ref line);
                    result.Add(entity);
                    break;

                default: throw new CsvMachineException("Unknown CsvMachine state");
            }
        }

        /// <summary>
        /// Process the final line if the file does not end with a line break
        /// </summary>
        internal List<T> EndOfFile()
        {
            if (_line.State != Line<T>.States.Initial)
            {
                _line.Process(Config.NewLine);
            }

            return result;
        }

        /// <summary>
        /// Sets the state machine to Content
        /// Precondition: Set the CSV index for the Properties
        /// </summary>
        internal void SetStateContent()
        {
            if (!_properties.Any(x => x.Index.HasValue))
            {
                throw new CsvMachineException("Property Index needs to be set first, before calling this method");
            }

            // Generate Factory
            _factory = new EntityFactory<T>(_properties, _lineActions);

            // Focus on content now
            State = States.Content;

            // Generate parser dictionary
            _parsers = _properties.Where(x => x.Index.HasValue).ToDictionary(x => x.Index!.Value, x => x.ValueParser!);
            foreach (var p in _parsers)
            {
                p.Value.Config = Config;
            }

            // Change the parser
            _line.SetParserAndResetState();
        }

        /// <summary>
        /// Get the appropriate parser or default to the string parser
        /// </summary>
        /// <param name="columnNumber">Current column number in the CSV line</param>
        /// <returns>The correct parser</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ValueParser GetParser(int columnNumber)
        {
            return _parsers!.TryGetValue(columnNumber, out var valueParser) ? valueParser : _stringParser;
        }

        #region Private

        /// <summary>
        /// Find Header Line
        /// </summary>
        /// <param name="fields">Parsed fields of the line</param>
        private void FindAndSetHeaders(IEnumerable<string?> fields)
        {
            if (_line.LineCounter >= Config.HeaderSearchLimit)
            {
                ThrowHelper.ThrowCsvMalformedException(
                    "Header not found in CSV file, please check your delimiter or the column definition!");
            }

            var headers = _properties.Select(x => x.ColumnName!);

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
            foreach (var p in _properties)
            {
                p.Index = headersDic[p.ColumnName!];
            }

            SetStateContent();
        }

        #endregion Private
    }
}