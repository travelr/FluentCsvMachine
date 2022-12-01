namespace FluentCsvMachine.Machine.Result
{
    readonly struct ResultLine
    {
        public ResultLine(ResultValue[] fields, int count, int lineNumber)
        {
            Fields = new ResultValue[count];
            Array.Copy(fields, Fields, count);
            LineNumber = lineNumber;
        }

        /// <summary>
        /// Parsed CSV fields
        /// </summary>
        public ResultValue[] Fields { get; }

        /// <summary>
        /// Line number in CSV file
        /// Required for keeping the order after multi threaded work
        /// </summary>
        public int LineNumber { get; }

        public static implicit operator ResultValue[](ResultLine line)
        {
            return line.Fields;
        }
    }
}