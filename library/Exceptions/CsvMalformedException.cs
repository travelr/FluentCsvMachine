using FluentCsvMachine.Helpers;

namespace FluentCsvMachine.Exceptions
{
    /// <summary>
    /// CSV file cannot be parsed
    /// </summary>
    public class CsvMalformedException : Exception
    {
        /// <summary>
        /// CSV file cannot be parsed
        /// </summary>
        /// <param name="message">Reason while the file cannot be parsed</param>
        public CsvMalformedException(string? message) : base(message)
        {
            Guard.IsNotNull(message);
        }
    }
}