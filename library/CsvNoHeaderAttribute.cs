namespace FluentCsvMachine
{
    /// <summary>
    /// Attribute for CSV files without header definitions
    /// Attribute used together with the ParseWithoutHeader functions
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class CsvNoHeaderAttribute : Attribute
    {
        /// <summary>
        /// Add this attribute to your property to define the column 
        /// </summary>
        /// <param name="columnIndex">
        /// CSV Column Index
        /// !Zero Based! Shall not be greater than the minimum number of columns defined on any line
        /// </param>
        /// <param name="inputFormat">InputFormat for DateTime, ...</param>
        /// <exception cref="ArgumentOutOfRangeException">columnIndex is negative</exception>
        public CsvNoHeaderAttribute(int columnIndex, string? inputFormat = null)
        {
            if (columnIndex < 0) throw new ArgumentOutOfRangeException(nameof(columnIndex));

            ColumnIndex = columnIndex;
            InputFormat = inputFormat;
        }

        /// <summary>
        /// Column Index
        /// </summary>
        public int ColumnIndex { get; }

        /// <summary>
        /// InputFormat
        /// </summary>
        public string? InputFormat { get; }
    }
}
