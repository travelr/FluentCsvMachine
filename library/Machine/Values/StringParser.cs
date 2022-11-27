using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Result;
using System.Buffers;

namespace FluentCsvMachine.Machine.Values
{
    internal class StringParser : ValueParser
    {
        private static readonly ArrayPool<char> ArrayPool = ArrayPool<char>.Shared;

        private int _i;
        private char[] _work;
        private readonly Type _resultType;

        public StringParser() : base(true)
        {
            _work = ArrayPool<char>.Shared.Rent(256);
            _i = 0;
            _resultType = typeof(string);
        }

        ~StringParser()
        {
            ArrayPool.Return(_work, true);
        }

        internal override void Process(char c)
        {
            if (_i == _work.Length)
            {
                var newSize = _work.Length * 2;
                if (newSize > 4096)
                {
                    ThrowHelper.ThrowCsvMalformedException("Column exceeds maximum length of 4096 bytes");
                }

                // Create a new array with doubled size
                char[] newArray = ArrayPool.Rent(newSize);
                Array.Copy(_work, 0, newArray, 0, _i);
                ArrayPool.Return(_work, true);
                _work = newArray;
            }

            _work[_i++] = c;
        }

        internal override ResultValue GetResult()
        {
            var str = new string(_work, 0, _i);

            var returnValue = _i > 0 ? new ResultValue(_resultType, str) : new ResultValue();

            _i = 0;
            return returnValue;
        }
    }
}