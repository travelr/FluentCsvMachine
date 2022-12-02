namespace FluentCsvMachine.Property
{
    /// <summary>
    /// Extensions which make the fluent definition of columns possible
    /// </summary>
    public static class CsvPropertyExtensions
    {
        /// <summary>
        /// Add the mandatory Column Name
        /// </summary>
        /// <param name="column">Current column</param>
        /// <param name="columnName">The name of the column, defined in the CSV header</param>
        /// <returns></returns>
        public static CsvPropertyBase ColumnName(this CsvPropertyBase column, string columnName)
        {
            column.ColumnName = columnName;
            return column;
        }

        /// <summary>
        /// Adds InputFormat which is required for DateTime columns
        /// </summary>
        /// <param name="column">Current column</param>
        /// <param name="inputFormat">Specific InputFormat, see for examples <seealso cref="DateTime.ParseExact(string, string, IFormatProvider?)"/></param>
        /// <returns></returns>
        public static CsvPropertyBase InputFormat(this CsvPropertyBase column, string inputFormat)
        {
            column.InputFormat = inputFormat;
            return column;
        }
    }
}