using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine;
using FluentCsvMachine.Machine.Values;
using FluentCsvMachine.Machine.Workflow;

namespace FluentCsvMachine.Property
{
    /// <summary>
    /// CsvPropertyBase corresponds to column in a CSV file
    /// Abstract class to have one common type for lists
    /// </summary>
    public abstract class CsvPropertyBase
    {
        /// <summary>
        /// See <see cref="CsvPropertyBase"/>
        /// </summary>
        /// <param name="propertyType">Property of the column</param>
        /// <param name="isCustom">Is a custom mapping column</param>
        protected CsvPropertyBase(Type propertyType, bool isCustom)
        {
            Guard.IsNotNull(propertyType);

            IsCustom = isCustom;

            PropertyType = propertyType;
        }

        /// <summary>
        /// False: CsvProperty, True: CsvPropertyCustom
        /// </summary>
        public bool IsCustom { get; }


        /// <summary>
        /// Name of the CSV column
        /// </summary>
        public string? ColumnName { get; set; }

        private int? _index;

        /// <summary>
        /// Column Index in CSV
        /// </summary>
        public int? Index
        {
            get => _index;
            internal set
            {
                if (value is < 0) throw new ArgumentOutOfRangeException(nameof(value));
                _index = value;
            }
        }

        /// <summary>
        /// Type of the property
        /// </summary>
        public Type? PropertyType { get; protected set; }


        /// <summary>
        /// InputFormat for DateTime, ...
        /// </summary>
        public string? InputFormat { get; set; }

        /// <summary>
        /// ValueParser, set in <see cref="WorkflowInput{T}.SetParsers"/> 
        /// </summary>
        internal ValueParser? ValueParser { get; set; }
    }
}