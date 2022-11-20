using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine;
using FluentCsvMachine.Machine.Workflow;
using FluentCsvMachine.Property;
using System.Linq.Expressions;

namespace FluentCsvMachine
{
    /// <summary>
    /// Fluent CSV Machine - an expressions based CSV parser
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
        /// <param name="path">Path to the CSV file</param>
        /// <param name="config">CsvConfiguration object, if not defined defaults are used</param>
        /// <returns>list of parsed objects</returns>
        public IReadOnlyList<T> Parse(string path, CsvConfiguration? config = null)
        {
            CheckPreconditions();
            if (!File.Exists(path))
            {
                ThrowHelper.ThrowFileNotFoundException(path);
            }

            using var fs = File.OpenRead(path);

            var result = StartWorkflow(fs, config);

            return result;
        }


        /// <summary>
        /// Parse the CSV File
        /// Assumes first line are headers
        /// </summary>
        /// <param name="stream">Stream of the CSV file</param>
        /// <param name="config">CsvConfiguration object, if not defined defaults are used</param>
        /// <returns>list of parsed objects</returns>
        public IReadOnlyList<T> ParseStream(Stream stream, CsvConfiguration? config = null)
        {
            CheckPreconditions();
            Guard.IsNotNull(stream);

            var result = StartWorkflow(stream, config);

            return result;
        }

        #region private

        private void CheckPreconditions()
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
        }

        private IReadOnlyList<T> StartWorkflow(Stream stream, CsvConfiguration? config = null)
        {
            var input = new WorkflowInput<T>(stream, properties)
            {
                Config = config,
                LineActions = _lineActions
            };

            var workflow = new Workflow<T>(input);
            var result = workflow.Start();

            return result;
        }

        #endregion private
    }
}