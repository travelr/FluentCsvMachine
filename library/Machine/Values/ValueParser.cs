using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Result;
using System.Runtime.CompilerServices;

namespace FluentCsvMachine.Machine.Values
{
    internal abstract class ValueParser
    {
        public enum States
        {
            Parsing,
            FastForward
        }

        protected readonly bool Nullable;
        protected bool IsNull;

        /// <summary>
        /// Abstract parser of an certain type
        /// </summary>
        /// <param name="nullable">Defines if the type is nullable</param>
        protected ValueParser(bool nullable)
        {
            Nullable = nullable;
            State = States.Parsing;
        }


        public States State { get; protected internal set; }

        /// <summary>
        /// Current CSV Configuration, should be set when the header was found
        /// </summary>
        /// <remarks>Do not use in a multi thread context</remarks>
        internal CsvConfiguration? Config { get; set; }

        /// <summary>
        /// Process character in CSV file
        /// </summary>
        /// <param name="c">Current character</param>
        internal abstract void Process(char c);

        /// <summary>
        /// CSV field ended, get the result
        /// Don't forget to reset variables
        /// </summary>
        /// <returns>ResultValue</returns>
        internal abstract ResultValue GetResult();

        /// <summary>
        /// Calculates the return type of ResultLine
        /// </summary>
        /// <typeparam name="T">Type of field</typeparam>
        /// <returns>Nullable or not nullable typeof(T)</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Type GetResultType<T>()
        {
            return Nullable ? typeof(Nullable<>).MakeGenericType(typeof(T)) : typeof(T);
        }

        /// <summary>
        /// Sets the current value as null
        /// Throws an Exception if it is not allowed
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetNull()
        {
            if (!Nullable)
            {
                // ToDo: Catch in Line and rethrow
                ThrowHelper.ThrowCsvMalformedException("Cannot convert null to 'type' because it is a non-nullable value type");
            }

            IsNull = true;
            State = States.FastForward;
        }
    }
}