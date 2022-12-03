namespace FluentCsvMachine.Machine.Result
{
    readonly struct ResultLine
    {
        public ResultLine(ref ResultValue[] fields, int count)
        {
            _array = new ResultValue[count];
            Array.Copy(fields, _array, count);
        }

        private readonly ResultValue[] _array;

        public ReadOnlySpan<ResultValue> AsSpan()
        {
            return _array;
        }

        public int Length => _array.Length;

        public ref readonly ResultValue this[int index] => ref _array[index];
    }
}