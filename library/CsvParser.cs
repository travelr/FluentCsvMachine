using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using readerFlu.Helpers;

namespace readerFlu
{
    /// <summary>
    /// CSV parsing and automated mapping
    /// Fluent property defintions
    /// </summary>
    /// <typeparam name="T">Type of </typeparam>
    public class CsvParser<T> where T : new()
    {
        private readonly List<CsvPropertyCustom> customMappingsColumn = new();
        private readonly List<Action<object, List<string>>> customMappingsLine = new();
        private readonly Dictionary<Type, List<CsvPropertyBase>> properties = new();

        /// <summary>
        /// Caches accessor expression to a lamda compiled setter
        /// </summary>
        private static readonly Dictionary<Expression<Func<T, object>>, Action<T, object>> setterCache = new();

        /// <summary>
        /// Defines a Column / Property
        /// </summary>
        /// <typeparam name="V">value type</typeparam>
        /// <param name="accessor"></param>
        /// <returns></returns>
        public CsvProperty<T, object> Property<V>(Expression<Func<T, object>> accessor)
        {
            var prop = new CsvProperty<T, object>(accessor);

            if (!properties.ContainsKey(typeof(V)))
            {
                properties.Add(typeof(V), new List<CsvPropertyBase>() { prop });
            }
            else
            {
                properties[typeof(V)].Add(prop);
            }

            return prop;
        }

        /// <summary>
        /// Defines a custom mapping based on a CSV column
        /// </summary>
        /// <param name="customAction">Action(Entity for value assignment, csv value)</param>
        /// <returns></returns>
        public CsvPropertyCustom CustomMappingColumn(Action<object, string> customAction)
        {
            var prop = new CsvPropertyCustom(customAction);
            customMappingsColumn.Add(prop);
            return prop;
        }

        /// <summary>
        /// Defines a custom mapping based on a full CSV line
        /// </summary>
        /// <param name="customAction">Action(Entity for value assignment, CSV line as List<string>)</param>
        public void CustomMappingLine(Action<object, List<string>> customAction)
        {
            customMappingsLine.Add(customAction);
        }

        /// <summary>
        /// Parse CSV File
        /// Assumes first line are headers
        /// </summary>
        /// <param name="path"></param>
        /// <param name="separator"></param>
        /// <param name="encoding"></param>
        /// <returns>list of parsed objects</returns>
        public List<T> Parse(string path, char separator = ';', Encoding? encoding = null)
        {
            // All object properties
            var allProperties = properties.SelectMany(x => x.Value).Concat(customMappingsColumn);

            // Parse CSV file
            var csvRaw = ParseRaw(path, separator, encoding);

            // Find Header Line
            var headerIndex = 0;
            var headerStrings = allProperties.Select(x => x.ColumnName).ToList();
            for (int i = 0; i < csvRaw.Count; i++)
            {
                // all header stirngs are included in the CSV line?
                if (headerStrings.All(x => csvRaw[i].Any(y => y.Equals(x))))
                {
                    break;
                }
                else
                {
                    headerIndex++;
                }
            }

            // Define Headers
            var headers = csvRaw[headerIndex];
            var headersDic = new Dictionary<string, int>();
            headers.ForEach((str, i) =>
            {
                if (!string.IsNullOrEmpty(str))
                {
                    headersDic.Add(str, i);
                }
            });

            // Set CSV row index for all properties
            allProperties.ForEach(x => x.SetIndex(headersDic));

            // Parse Objects
            // CSV lines after the header where the column count matches the header count
            var result = new List<T>();
            var content = csvRaw.Skip(headerIndex + 1).Where(line => line.Count >= headersDic.Count);
            foreach (var line in content)
            {
                var resultObj = new T();

                // Set the properties of the object based on the CSV line
                SetProperties(line, resultObj);
                // Set remaming properties which require custom mappings
                RunCustomMappings(line, resultObj);

                result.Add(resultObj);
            }

            return result;
        }

        /// <summary>
        /// Runs customMappingsColumn or customMappingsLine
        /// </summary>
        /// <param name="line">Current CSV line</param>
        /// <param name="resultObj">Entity for value assignment</param>
        /// <exception cref="ArgumentNullException">Entity is null</exception>
        private void RunCustomMappings(List<string> line, T resultObj)
        {
            if (resultObj == null)
                throw new ArgumentNullException(nameof(resultObj));

            // Column mappings
            foreach (var cm in customMappingsColumn)
            {
                // Raw value form CSV
                var valueRaw = line[cm.Index];

                cm.CustomAction(resultObj, valueRaw);
            }

            // Line mappings
            foreach (var cm in customMappingsLine)
            {
                cm(resultObj, line);
            }
        }

        #region private methods

        private static readonly Dictionary<Type, Func<string, CsvPropertyBase, object>> converterFunctions = new()
        {
            { typeof(string), (x, b) => x },
            {
                typeof(DateTime),
                (x, b) =>
                {
                    DateTime value;
                    if (b.InputFormat == null)
                    {
                        value = DateTime.Parse(x);
                    }
                    else
                    {
                        value = DateTime.ParseExact(x, b.InputFormat, null);
                    }

                    return value;
                }
            },
            { typeof(decimal), (x, b) => decimal.Parse(x) },
            {
                typeof(bool),
                (x, b) => x == "Ja" //ToDo: Better boolean parsing
            },
        };

        private List<List<string>> ParseRaw(string path, char separator = ';', Encoding? encoding = null)
        {
            // Check preconditions
            if (!properties.Any())
            {
                throw new Exception("define properties first");
            }
            else if (!File.Exists(path))
            {
                throw new FileNotFoundException(path);
            }

            // Set up
            var result = new List<List<string>>();
            var csvCleanUp = new char[] { separator, '"' };

            // Read CSV
            using var reader = new StreamReader(File.OpenRead(path), encoding ?? Encoding.GetEncoding("ISO-8859-1"));
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line)) continue;

                var values = line.Split(separator).Select(x => x.Trim(csvCleanUp)).ToList();
                result.Add(values);
            }

            // Short validation
            if (result.Count < 2 || result[0].Count == 0)
            {
                throw new Exception("parsing seems to have failed");
            }

            return result;
        }

        /// <summary>
        /// Sets the properties of the object based on the CSV line
        /// </summary>
        /// <param name="line">CSV line</param>
        /// <param name="resultObj">corresponding object</param>
        /// <exception cref="Exception">CsvProperty has no property Accessor</exception>
        private void SetProperties(List<string> line, T resultObj)
        {
            foreach (var kv in properties)
            {
                var propertyType = kv.Key;
                var properties = kv.Value;

                // get Accessor property
                var accessorProperty = typeof(CsvProperty<,>).MakeGenericType(typeof(T), typeof(object)).GetProperty("Accessor");
                if (accessorProperty == null)
                {
                    throw new Exception("Accessor property has been renamed!");
                }

                foreach (var p in properties)

                {
                    if (accessorProperty.GetValue(p, null) is not Expression<Func<T, object>> accessor)
                    {
                        throw new Exception("accessor is null");
                    }

                    // Raw value form CSV
                    var valueRaw = line[p.Index];

                    // Convert raw value based on defined methods
                    var value = converterFunctions[propertyType](valueRaw, p);

                    // Assign to value to the object
                    var setter = CsvParser<T>.GetSetter(accessor);
                    setter(resultObj, value);
                }
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
                throw new Exception("unkown expression type");
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

        #endregion private methods
    }
}