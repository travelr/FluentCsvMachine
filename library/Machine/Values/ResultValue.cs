namespace FluentCsvMachine.Machine.Values
{
    /// <summary>
    /// Result of a ValueParser
    /// </summary>
    internal class ResultValue
    {
        public ResultValue(Type t, object value)
        {
            Type = t;
            Value = value;
        }

        /// <summary>
        /// Type of the CSV column (field)
        /// </summary>
        internal Type Type { get; }

        /// <summary>
        /// Value of the CSV column (field)
        /// </summary>
        internal object Value { get; }
    }
}