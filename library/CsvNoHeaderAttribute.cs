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
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public CsvNoHeaderAttribute(int columnIndex)
        {
            if (columnIndex < 0) throw new ArgumentOutOfRangeException(nameof(columnIndex));

            ColumnIndex = columnIndex;
        }

        public int ColumnIndex { get; }
    }
}
