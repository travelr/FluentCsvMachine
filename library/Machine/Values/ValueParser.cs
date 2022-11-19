using FluentCsvMachine.Machine.Result;

namespace FluentCsvMachine.Machine.Values
{
    internal abstract class ValueParser
    {
        public enum States
        {
            Parsing,
            FastForward
        }

        public States State { get; protected internal set; } = States.Parsing;

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
        /// </summary>
        /// <returns>ResultValue</returns>
        internal abstract ResultValue GetResult();
    }
}