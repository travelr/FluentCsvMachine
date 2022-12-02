namespace FluentCsvMachine.Machine.Result
{
    readonly struct ResultLine
    {
    
        public ResultLine(ref ResultValue[] fields, int count, int lineNumber)
        {
            _array = new ResultValue[count];
            Array.Copy(fields, _array, count);
            LineNumber = lineNumber;
        }

        private readonly ResultValue[] _array;

        public ReadOnlySpan<ResultValue> AsSpan()
        {
            return _array;
        }

        /// <summary>
        /// Line number in CSV file
        /// Required for keeping the order after multi threaded work
        /// </summary>
        public int LineNumber { get; }

        public int Length => _array.Length;

        public ref readonly ResultValue this[int index] => ref _array[index];
    }
}