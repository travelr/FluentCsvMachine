using FluentCsvMachine.Exceptions;
using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Values;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentCsvMachine.Machine
{
    internal class CsvMachine<T> where T : new()
    {
        internal enum States
        {
            HeaderSearch,
            Content
        }

        private readonly Line<T> Line;
        private readonly List<CsvPropertyBase> _properties;

        private readonly List<T> result;

        internal States State { get; private set; }

        private Dictionary<int, ValueParser>? _parsers;
        private readonly StringParser _stringParser = new();

        public CsvConfiguration Config { get; }

        /// <summary>
        /// Caches accessors expression to a lambda compiled setter
        /// </summary>
        private static readonly Dictionary<Expression<Func<T, object>>, Action<T, object>> setterCache = new();

        internal CsvMachine(CsvConfiguration config, List<CsvPropertyBase> properties)
        {
            Guard.IsNotNull(config);
            Guard.IsNotNull(properties);

            Config = config;
            State = States.HeaderSearch;

            Line = new Line<T>(this);

            _properties = properties;
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

                Line.Process(buffer[i]);
            }
        }

        /// <summary>
        /// A full line was parsed, here is the result
        /// </summary>
        /// <param name="line">The line, empty strings are null</param>
        internal void ResultLine(List<ResultValue?> line)
        {
            switch (State)
            {
                case States.HeaderSearch:
                    // Header Search is only using string
                    var strLine = line.Select(x => x != null ? (string)x.Value : null);
                    FindAndSetHeaders(strLine);
                    break;

                case States.Content:
                    CreateEntity(line);
                    break;

                default: throw new CsvMachineException();
            }
        }

        /// <summary>
        /// Process the final line if the file does not end with a line break
        /// </summary>
        internal List<T> EndOfFile()
        {
            if (Line.State != Line<T>.States.Initial)
            {
                Line.Process(Config.NewLine);
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
            if (Line.LineCounter >= Config.HeaderSearchLimit)
            {
                ThrowHelper.ThrowCsvMalformedException("Header not found in CSV file, please check your delimiter or the column definition!");
            }

            var headers = _properties.Select(x => x.ColumnName!);

            if (headers.All(x => fields.Any(y => y != null && x == y)))
            {
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

                // Focus on content now
                State = States.Content;

                // Generate parser dictionary
                _parsers = _properties.Where(x => x.Index.HasValue).ToDictionary(x => x.Index!.Value, x => x.ValueParser!);
                foreach (var p in _parsers.Where(x => x.Value != null))
                {
                    p.Value.Config = Config;
                }
            }
        }

        private void CreateEntity(List<ResultValue?> line)
        {
            var resultObj = new T();

            // Set the properties of the object based on the CSV line
            SetProperties(line, resultObj);
            // Set remaining properties which require custom mappings
            //RunCustomMappings(line, resultObj);

            result.Add(resultObj);
        }

        /// <summary>
        /// Sets the properties of the object based on the CSV line
        /// </summary>
        /// <param name="line">CSV line</param>
        /// <param name="resultObj">corresponding object</param>
        /// <exception cref="Exception">CsvProperty has no property Accessor</exception>
        private void SetProperties(List<ResultValue?> line, T resultObj)
        {
            foreach (var p in _properties)
            {
                // get Accessor property
                var accessorProperty = typeof(CsvProperty<>).MakeGenericType(typeof(T)).GetProperty("Accessor");
                if (accessorProperty == null)
                {
                    throw new Exception("Accessor property has been renamed!");
                }

                if (accessorProperty.GetValue(p, null) is not Expression<Func<T, object>> accessor)
                {
                    throw new Exception("accessor is null");
                }

                // Raw value form CSV
                var resultValue = line[p.Index!.Value];
                if (resultValue == null)
                {
                    continue;
                }

                // Convert raw value based on defined methods
                var value = Convert.ChangeType(resultValue.Value, resultValue.Type);

                // Assign to value to the object
                var setter = GetSetter(accessor);
                setter(resultObj, value);
            }
        }

        /// <summary>
        /// Creates or gets a setter to a setter
        /// </summary>
        /// <param name="accessor">Accessor expression to the property</param>
        /// <returns>The setter</returns>
        /// <exception cref="Exception">Should not happen, broken code?</exception>
        private static Action<T, object> GetSetter(Expression<Func<T, object>> accessor)
        {
            // Try to get a cached setter
            if (setterCache.TryGetValue(accessor, out var result))
            {
                return result;
            }

            // Try get to get the property
            MemberExpression? selectorExpr = accessor.Body as MemberExpression;
            if (selectorExpr == null)
            {
                var unary = accessor.Body as UnaryExpression;
                selectorExpr = unary?.Operand as MemberExpression;
            }

            // Double check if everything could be resolved
            if (selectorExpr == null || selectorExpr.Member is not PropertyInfo property || property.DeclaringType == null)
            {
                throw new Exception("unknown expression type");
            }

            // Based on https://stackoverflow.com/a/17669142
            var exInstance = Expression.Parameter(property.DeclaringType, "t");
            var exMemberAccess = Expression.MakeMemberAccess(exInstance, property);
            var exValue = Expression.Parameter(typeof(object), "p");
            var exConvertedValue = Expression.Convert(exValue, property.PropertyType);
            var exBody = Expression.Assign(exMemberAccess, exConvertedValue);
            var lambda = Expression.Lambda<Action<T, object>>(exBody, exInstance, exValue);
            var action = lambda.Compile();

            setterCache.Add(accessor, action);
            return action;
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