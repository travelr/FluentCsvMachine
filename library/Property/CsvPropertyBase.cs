using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Values;

namespace FluentCsvMachine.Property
{
    /// <summary>
    /// CsvPropertyBase corresponds to column in a CSV file
    /// Abstract class to have one common type for lists
    /// </summary>
    public abstract class CsvPropertyBase
    {
        /// <summary>
        /// Type of the property
        /// Is null on actions
        /// </summary>
        public Type? PropertyType { get; protected set; }

        /// <summary>
        /// Name of the CSV column
        /// </summary>
        public string? ColumnName { get; set; }

        /// <summary>
        /// InputFormat for DateTime, ...
        /// </summary>
        public virtual string? InputFormat { get; set; }

        /// <summary>
        /// Column Index in CSV
        /// </summary>
        public int? Index { get; private set; }

        internal ValueParser? ValueParser { get; set; }

        /// <summary>
        /// Reference Column Index in CSV
        /// </summary>
        /// <param name="headersDic">Column Name, Index</param>
        public void SetIndex(Dictionary<string, int> headersDic)
        {
            Guard.IsNotNull(headersDic);

            Index = headersDic[ColumnName!];
        }
    }
}