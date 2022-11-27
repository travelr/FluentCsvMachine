namespace FluentCsvMachine.Machine.Result
{
    readonly struct ResultLine
    {
        public ResultLine(ResultValue[] fields, int count)
        {
            Fields = new ResultValue[count];
            Array.Copy(fields, Fields, count);
        }

        public ResultValue[] Fields { get; }

        public static implicit operator ResultValue[](ResultLine line)
        {
            return line.Fields;
        }
    }
}