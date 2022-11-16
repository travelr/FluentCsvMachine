using FluentCsvMachine.Machine;
using FluentCsvMachine.Machine.Values;
using System.Linq.Expressions;

namespace FluentCsvMachine
{
    /// <summary>
    /// CsvProperty correponds to column in a CSV file
    /// </summary>
    /// <typeparam name="T">This property belongs to T type</typeparam>
    /// <typeparam name="V">Value type of the property</typeparam>
    public class CsvProperty<T> : CsvPropertyBase
    {
        public CsvProperty(Type propertyType, Expression<Func<T, object?>> accessor)
        {
            PropertyType = propertyType;
            Accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));

            if (propertyType != typeof(DateTime))
            {
                // DateTime requires InputFormat
                ValueParser = ValueParserProvider.GetParser(propertyType);
            }
        }

        private string? _inputFormat;

        public override string? InputFormat
        {
            get => _inputFormat;
            set
            {
                _inputFormat = value;

                if (PropertyType == typeof(DateTime))
                {
                    ValueParser = ValueParserProvider.GetParser(PropertyType, value);
                }
            }
        }

        /// <summary>
        /// Accessor of the property
        /// </summary>
        public Expression<Func<T, object?>> Accessor { get; private set; }
    }
}