using FluentCsvMachine.Helpers;

namespace FluentCsvMachine.Exceptions
{
    /// <summary>
    /// "The column {property.Name} has the type ({targetPropertyType}) which does not match the class definition ({property.PropertyType})"
    /// </summary>
    public class CsvColumnMismatchException : Exception
    {
        public CsvColumnMismatchException(string message) : base(message)
        {
            Guard.IsNotNull(message);
        }
    }
}
