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
    /// <typeparam name="T">Type of the entity which represents a CSV line</typeparam>
    public class CsvParser<T> where T : new()
    {
        private readonly List<CsvPropertyBase> properties = new();
        private readonly List<Action<T, IReadOnlyList<object?>>> _lineActions = new();

        /// <summary>
        /// Defines a Column / Property
        /// </summary>
        /// <typeparam name="V">value type</typeparam>
        /// <param name="accessor"></param>
        /// <returns>Property for the fluent interface</returns>
        public CsvProperty<T> Property<V>(Expression<Func<T, object?>> accessor)
        {
            Guard.IsNotNull(accessor);

            var prop = new CsvProperty<T>(typeof(V), accessor);

            properties.Add(prop);

            return prop;
        }

        /// <summary>
        /// Defines a custom mapping based on a type
        /// Will be executed after all normal Properties
        /// Can assign modify multiple properties of the entity if you want to do that
        /// Input is always the parsed value of the CSV column in the type V
        /// </summary>
        /// <param name="customAction">Action(Entity for value assignment, parsed csv value) </param>
        /// <returns>Property for the fluent interface</returns>
        public CsvPropertyCustom<T, V> PropertyCustom<V>(Action<T, V> customAction)
        {
            Guard.IsNotNull(customAction);

            var prop = new CsvPropertyCustom<T, V>(typeof(V), customAction);

            properties.Add(prop);

            return prop;
        }

        /// <summary>
        /// Defines an action which runs after all properties (normal as well as custom ones) have been mapped
        /// </summary>
        /// <param name="lineAction">
        /// Action(Entity, List of parsed csv fields)
        /// List is represents the CSV columns which have a column defined, others will be null
        /// The index matches the CSV columns
        /// The type of the object in the list is based on the columns you have defined
        /// These actions will be executed in the end of the entity creation
        /// Use this method only if PropertyCustom is not able to do what you want to achieve 
        /// </param>
        public void LineAction(Action<T, IReadOnlyList<object?>> lineAction)
        {
            Guard.IsNotNull(lineAction);
            _lineActions.Add(lineAction);
        }

        /// <summary>
        /// Parse the CSV File
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
            var csv = new CsvMachine<T>(config, properties, _lineActions);

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