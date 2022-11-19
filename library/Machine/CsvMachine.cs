using FluentCsvMachine.Exceptions;
using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Values;
using FluentCsvMachine.Property;

namespace FluentCsvMachine.Machine
{
    internal class CsvMachine<T> where T : new()
    {
        internal enum States
        {
            HeaderSearch,
            Content
        }

        private readonly Line<T> _line;
        private readonly List<CsvPropertyBase> _properties;

        private readonly List<T> result;

        internal States State { get; private set; }

        private Dictionary<int, ValueParser>? _parsers;
        private readonly StringParser _stringParser = new();

        public CsvConfiguration Config { get; }


        /// <summary>
        /// Factory for entities
        /// Assign only after headers are found because the Index value is required
        /// </summary>
        private EntityFactory<T>? _factory;

        private readonly List<Action<T, IReadOnlyList<object?>>> _lineActions;

        internal CsvMachine(CsvConfiguration config, List<CsvPropertyBase> properties, List<Action<T, IReadOnlyList<object?>>> lineActions)
        {
            Guard.IsNotNull(config);
            Guard.IsNotNull(properties);

            Config = config;
            State = States.HeaderSearch;

            _line = new Line<T>(this);

            _properties = properties;
            _lineActions = lineActions;

            result = new List<T>();
        }

        internal void Process(char[] buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (buffer[i] == '\r')
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
        internal void ResultLine(List<ResultValue> line)
        {
            switch (State)
            {
                case States.HeaderSearch:
                    // Header Search is only using string
                    var strLine = line.Select(x => !x.IsNull ? (string)x.Value! : null);
                    FindAndSetHeaders(strLine);
                    break;

                case States.Content:
                    var entity = _factory!.Create(line);
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
            _properties.ForEach(x => x.SetIndex(headersDic));

            // Generate Factory
            _factory = new EntityFactory<T>(_properties, _lineActions);

            // Focus on content now
            State = States.Content;

            // Generate parser dictionary
            _parsers = _properties.Where(x => x.Index.HasValue).ToDictionary(x => x.Index!.Value, x => x.ValueParser!);
            foreach (var p in _parsers.Where(x => x.Value != null))
            {
                p.Value.Config = Config;
            }
        }


        internal ValueParser GetParser(int columnNumber)
        {
            if (_parsers!.TryGetValue(columnNumber, out var result))
            {
                return result;
            }
            else
            {
                // ToDo: Is skip working?
                return _stringParser;
            }
        }

        #endregion Private
    }
}