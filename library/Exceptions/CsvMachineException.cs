namespace FluentCsvMachine.Exceptions
{
    /// <summary>
    /// Internal exception: State machines cannot handle CSV file
    /// </summary>
    public class CsvMachineException : Exception
    {
        /// <summary>
        /// Internal exception: State machines cannot handle CSV file -> Bug Ticket is needed
        /// </summary>
        /// <param name="message">Reason of the failure</param>
        public CsvMachineException(string message) : base(
            $"FluentCsvMachine cannot handle the CSV file, please submit a bug ticket! Reason: {message}")
        {
        }
    }
}