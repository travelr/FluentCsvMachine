using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine;
using FluentCsvMachine.Machine.Values;

namespace FluentCsvMachine.Property
{
    /// <summary>
    /// CsvPropertyBase corresponds to column in a CSV file
    /// Abstract class to have one common type for lists
    /// </summary>
    public abstract class CsvPropertyBase
    {
        protected CsvPropertyBase(Type propertyType, bool isCustom)
        {
            Guard.IsNotNull(propertyType);

            IsCustom = isCustom;

            PropertyType = propertyType;
            if (propertyType != typeof(DateTime) && propertyType != typeof(DateTime?))
            {
                // DateTime requires InputFormat
                // Therefore it is set by InputFormat.Set
                ValueParser = ValueParserProvider.GetParser(propertyType);
            }
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


        private string? _inputFormat;

        /// <summary>
        /// InputFormat for DateTime, ...
        /// </summary>
        public string? InputFormat
        {
            get => _inputFormat;
            set
            {
                _inputFormat = value;

                if (PropertyType == typeof(DateTime) || PropertyType == typeof(DateTime?))
                {
                    ValueParser = ValueParserProvider.GetParser(PropertyType, value);
                }
            }
        }

        internal ValueParser? ValueParser { get; set; }
    }
}