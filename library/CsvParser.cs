using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine;
using System.Linq.Expressions;

namespace FluentCsvMachine
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
        private readonly List<CsvPropertyBase> properties = new();

        /// <summary>
        /// Defines a Column / Property
        /// </summary>
        /// <typeparam name="V">value type</typeparam>
        /// <param name="accessor"></param>
        /// <returns></returns>

        public CsvProperty<T> Property<V>(Expression<Func<T, object?>> accessor)
        {
            Guard.IsNotNull(accessor);

            var prop = new CsvProperty<T>(typeof(V), accessor);
            if (properties.Any(x => x.ColumnName == prop.ColumnName))
            {
                ThrowHelper.ThrowCsvConfigurationException("Duplicate column names");
            }

            properties.Add(prop);

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
        public List<T> Parse(string path, CsvConfiguration? config = null)
        {
            // Check preconditions
            if (!properties.Any())
            {
                ThrowHelper.ThrowCsvConfigurationException("Please define your properties first.");
            }
            else if (properties.Any(x => string.IsNullOrEmpty(x.ColumnName)))
            {
                ThrowHelper.ThrowCsvConfigurationException("Please make sure that all properties have a valid ColumnName set.");
            }
            else if (!File.Exists(path))
            {
                ThrowHelper.ThrowFileNotFoundException(path);
                throw new FileNotFoundException(path);
            }

            config ??= new CsvConfiguration();
            var csv = new CsvMachine<T>(config, properties);

            using var fs = File.OpenRead(path);
            using var sr = new StreamReader(fs, config.Encoding);

            char[] buffer = new char[config.BufferSize];
            int read = 0;

            while ((read = sr.Read(buffer, 0, buffer.Length)) > 0)
            {
                csv.Process(buffer, read);
            }

            return csv.EndOfFile();
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
    }
}