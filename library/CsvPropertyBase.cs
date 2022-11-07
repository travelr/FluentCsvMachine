using readerFlu.Helpers;

namespace readerFlu
{
    /// <summary>
    /// CsvPropertyBase correponds to column in a CSV file
    /// Abstract class to have one common type for lists
    /// </summary>
    public abstract class CsvPropertyBase
    {
        /// <summary>
        /// Name of the CSV column
        /// </summary>
        public string? ColumnName { get; set; }

        /// <summary>
        /// InputFormat for DateTime, ...
        /// </summary>
        public string? InputFormat { get; set; }

        /// <summary>
        /// Column Index in CSV
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// Reference Column Index in CSV
        /// </summary>
        /// <param name="headersDic">Colunm Name, Index</param>
        public void SetIndex(Dictionary<string, int> headersDic)
        {
            if (ColumnName == null)
            {
                throw new Exception("set column name first");
            }
            else if (headersDic.IsEmpty())
            {
                throw new ArgumentException(null, nameof(headersDic));
            }

            Index = headersDic[ColumnName];
        }
    }
}