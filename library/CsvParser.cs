using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine;
using FluentCsvMachine.Machine.Workflow;
using FluentCsvMachine.Property;
using System.Linq.Expressions;
using System.Reflection;

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
        private bool _setupNoHeadersComplete;

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

        #region Parse

        /// <summary>
        /// Parses the passed CSV File
        /// A header does need to exist in the CSV file
        /// Please setup your configuration with <see cref="Property"/> and <seealso cref="PropertyCustom{V}"/>,
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="config">CsvConfiguration object, if not defined defaults are used</param>
        /// <returns>List of parsed objects of type T</returns>
        public IReadOnlyList<T> Parse(string path, CsvConfiguration? config = null)
        {
            CheckPreconditionsWithHeader();

            using var fs = OpenFile(path);

            var result = StartWorkflow(fs, true, config);

            return result;
        }

        /// <summary>
        /// Parses the passed CSV stream object
        /// A header does need to exist in the CSV file
        /// Please setup your configuration with <see cref="Property"/> and <seealso cref="PropertyCustom{V}"/>,
        /// </summary>
        /// <param name="stream">Stream of the CSV file</param>
        /// <param name="config">CsvConfiguration object, if not defined defaults are used</param>
        /// <returns>List of parsed objects of type T</returns>
        public IReadOnlyList<T> ParseStream(Stream stream, CsvConfiguration? config = null)
        {
            Guard.IsNotNull(stream);
            CheckPreconditionsWithHeader();

            var result = StartWorkflow(stream, true, config);

            return result;
        }

        /// <summary>
        /// Parses the passed CSV file which does not contain a header
        /// Therefore you need setup your configuration with <see cref="CsvNoHeaderAttribute"/> and set the Index of the column directly
        /// Not allowed is the use of <see cref="Property"/> and <seealso cref="PropertyCustom{V}"/> because you cannot set the Property.ColumnName
        /// </summary>
        /// <param name="path">Path to the CSV file</param>
        /// <param name="config">CsvConfiguration object, if not defined defaults are used</param>
        /// <returns>List of parsed objects of type T</returns>
        public IReadOnlyList<T> ParseWithoutHeader(string path, CsvConfiguration? config = null)
        {
            SetupAndCheckNoHeaders();

            using var fs = OpenFile(path);

            var result = StartWorkflow(fs, false, config);

            return result;
        }

        /// <summary>
        /// Parses the passed CSV stream which does not contain a header.
        /// Therefore you need setup your configuration with the <see cref="CsvNoHeaderAttribute"/> on the properties of <typeparamref name="T"/> and set the index of the column directly.
        /// Not allowed is the use of <see cref="Property"/> and <seealso cref="PropertyCustom{V}"/> because you cannot set the Property.ColumnName
        /// </summary>
        /// <param name="stream">Stream of the CSV file</param>
        /// <param name="config">CsvConfiguration object, if not defined defaults are used</param>
        /// <returns>List of parsed objects of type T</returns>
        public IReadOnlyList<T> ParseWithoutHeader(Stream stream, CsvConfiguration? config = null)
        {
            Guard.IsNotNull(stream);

            SetupAndCheckNoHeaders();

            var result = StartWorkflow(stream, false, config);

            return result;
        }

        #endregion Parse


        #region private

        private void CheckPreconditionsWithHeader()
        {
            // Check preconditions
            if (_setupNoHeadersComplete)
            {
                ThrowHelper.ThrowCsvConfigurationException("Please create a new parser, mixed used for CSV files with and without headers is not supported.");
            }
            else if (!properties.Any())
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

        /// <summary>
        /// Used by ParseWithoutHeader
        /// </summary>
        private void SetupAndCheckNoHeaders()
        {
            if (_setupNoHeadersComplete)
            {
                return;
            }
            else if (properties.Any())
            {
                ThrowHelper.ThrowCsvConfigurationException("Please just use CsvNoHeaderAttributes and define no columns, because no csv header does exist.");
            }

            // Find Properties with CsvNoHeaderAttribute defined in T
            var propsWithAttributes = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    Property = p,
                    Attribute = (CsvNoHeaderAttribute?)p.GetCustomAttributes(typeof(CsvNoHeaderAttribute)).SingleOrDefault()
                }).Where(p => p.Attribute != null).ToList();

            // Validate CsvNoHeaderAttributes
            if (!propsWithAttributes.Any())
            {
                ThrowHelper.ThrowCsvConfigurationException($"Please set CsvNoHeaderAttributes in the class {typeof(T).Name}");
            }
            else if (propsWithAttributes.Select(x => x.Attribute!.ColumnIndex).Distinct().Count() != propsWithAttributes.Count)
            {
                ThrowHelper.ThrowCsvConfigurationException(
                    $"Please make sure that all column indexes are unique in the class {typeof(T).Name}");
            }

            var exInstance = Expression.Parameter(propsWithAttributes[0].Property.DeclaringType!, "t");

            // Create CsvProperty by reflection
            foreach (var x in propsWithAttributes)
            {
                // Create the expression for the property
                var body = Expression.Property(exInstance, x.Property);
                var convert = Expression.Convert(body, typeof(object));
                var lambda = Expression.Lambda<Func<T, object?>>(convert, exInstance);

                // Create an CsvProperty
                var csvProperty = new CsvProperty<T>(x.Property.PropertyType, lambda)
                {
                    // Set the values from the attribute
                    Index = x.Attribute!.ColumnIndex,
                    InputFormat = x.Attribute!.InputFormat
                };
                properties.Add(csvProperty);
            }

            _setupNoHeadersComplete = true;
        }

        private static FileStream OpenFile(string path)
        {
            if (!File.Exists(path))
            {
                ThrowHelper.ThrowFileNotFoundException(path);
            }

            return File.OpenRead(path);
        }

        private IReadOnlyList<T> StartWorkflow(Stream stream, bool searchForHeaders, CsvConfiguration? config = null)
        {
            var input = new WorkflowInput<T>(stream, properties, searchForHeaders)
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