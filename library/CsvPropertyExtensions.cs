using System.Linq.Expressions;
using System.Reflection;

namespace readerFlu
{
    public static class CsvPropertyExtensions
    {
        public static CsvPropertyBase ColumnName(this CsvPropertyBase column, string columnName)
        {
            column.ColumnName = columnName;
            return column;
        }

        public static CsvPropertyBase InputFormat(this CsvPropertyBase column, string inputFormat)
        {
            column.InputFormat = inputFormat;
            return column;
        }
    }
}