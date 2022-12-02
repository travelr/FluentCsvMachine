using FluentCsvMachine.Helpers;

namespace FluentCsvMachine.Exceptions
{
    /// <summary>
    /// Setup for parsing the CSV file is not correct
    /// </summary>
    public class CsvConfigurationException : Exception
    {
        /// <summary>
        /// CsvConfigurationException: Setup for parsing the CSV file is not correct
        /// </summary>
        /// <param name="message">Message about the issue</param>
        public CsvConfigurationException(string? message) : base(message)
        {
            Guard.IsNotNull(message);
        }
    }
}