namespace FluentCsvMachine.Machine.States
{
    /// <summary>
    /// Avoid slow getters provide fields
    /// </summary>
    internal class BaseElement
    {
        protected readonly char Delimiter;
        protected readonly char NewLine;
        protected readonly char Quote;
        protected readonly char QuoteEscape;
        protected readonly char? Comment;

        public BaseElement(CsvConfiguration config)
        {
            Delimiter = config.Delimiter;
            NewLine = config.NewLine;
            Quote = config.Quote;
            QuoteEscape = config.QuoteEscape;
            Comment = config.Comment;
        }
    }
}