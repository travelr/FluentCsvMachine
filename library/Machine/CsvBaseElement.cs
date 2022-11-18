namespace FluentCsvMachine.Machine
{
    /// <summary>
    /// Avoid slow getters provide fields
    /// </summary>
    internal class CsvBaseElement
    {
        protected readonly char Delimiter;
        protected readonly char NewLine;
        protected readonly char Quote;
        protected readonly char? Comment;

        public CsvBaseElement(CsvConfiguration config)
        {
            Delimiter = config.Delimiter;
            NewLine = config.NewLine;
            Quote = config.Quote;
            Comment = config.Comment;
        }
    }
}