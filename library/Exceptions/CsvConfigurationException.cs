using FluentCsvMachine.Helpers;

namespace FluentCsvMachine.Exceptions
{
    /// <summary>
    /// Setup for parsing the CSV file is not correct
    /// </summary>
    public class CsvConfigurationException : Exception
    {
        public CsvConfigurationException(string? message) : base(message)
        {
            Guard.IsNotNull(message);
        }
    }
}