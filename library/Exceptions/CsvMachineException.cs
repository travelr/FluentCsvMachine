namespace FluentCsvMachine.Exceptions
{
    /// <summary>
    /// Internal exception: State machines cannot handle CSV file
    /// </summary>
    public class CsvMachineException : Exception
    {
        public CsvMachineException(string message) : base(
            $"FluentCsvMachine cannot handle the CSV file, please submit a bug ticket! Reason: {message}")
        {
        }
    }
}