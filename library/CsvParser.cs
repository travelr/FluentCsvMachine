using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine;
using FluentCsvMachine.Property;
using System.Linq.Expressions;

namespace FluentCsvMachine
{
    /// <summary>
    /// CSV parsing and automated mapping
    /// Fluent property definitions
    /// </summary>
    /// <typeparam name="T">Type of </typeparam>
    public class CsvParser<T> where T : new()
    {
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

            properties.Add(prop);

            return prop;
        }

        /// <summary>
        /// Defines a custom mapping based on a CSV column
        /// </summary>
        /// <param name="accessor"></param>
        /// <param name="customAction">Action(Entity for value assignment, csv value) </param>
        /// <returns></returns>
        public CsvPropertyCustom<T, V> PropertyCustom<V>(Expression<Func<T, object?>> accessor, Action<T, V> customAction)
        {
            Guard.IsNotNull(accessor);

            var prop = new CsvPropertyCustom<T, V>(typeof(V), customAction);

            properties.Add(prop);

            return prop;
        }

        /// <summary>
        /// Parse CSV File
        /// Assumes first line are headers
        /// </summary>
        /// <param name="path"></param>
        /// <param name="config">CsvConfiguration object, if not defined defaults are used</param>
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
                ThrowHelper.ThrowCsvConfigurationException("Please make sure that all properties have a valid ColumnName.");
            }
            else if (properties.Select(x => x.ColumnName).Distinct().Count() != properties.Count)
            {
                ThrowHelper.ThrowCsvConfigurationException("Please make sure that all properties have an unique ColumnName.");
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
    }
}