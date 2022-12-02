using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine;
using FluentCsvMachine.Property;

namespace FluentCsvMachine.Exceptions
{
    /// <summary>
    /// Mismatch between the T of <see cref="CsvProperty{T}"/> and the property defined in the class <see cref="CsvMachine{T}"/> by the CSV Machine
    /// </summary>
    public class CsvColumnMismatchException : Exception
    {
        /// <summary>
        /// see <see cref="CsvColumnMismatchException"/>
        /// </summary>
        /// <param name="message"> "The column {property.Name} has the type ({targetPropertyType}) which does not match the class definition ({property.PropertyType})"</param>
        public CsvColumnMismatchException(string message) : base(message)
        {
            Guard.IsNotNull(message);
        }
    }
}
