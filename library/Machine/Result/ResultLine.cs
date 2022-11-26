namespace FluentCsvMachine.Machine.Result
{
    readonly struct ResultLine
    {
        public ResultLine()
        {
        }


        public int LineNumber { get; }

        public ResultValue[] Fields { get; }
    }
}
