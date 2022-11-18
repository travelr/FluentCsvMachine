namespace FluentCsvMachine.Machine.Values
{
    /// <summary>
    /// Result of a ValueParser
    /// </summary>
    readonly struct ResultValue
    {
        public ResultValue()
        {
            IsNull = true;
        }

        public ResultValue(Type t, object value)
        {
            Type = t;
            Value = value;
        }

        /// <summary>
        /// Type and Value are null
        /// structs cannot be null
        /// </summary>
        internal bool IsNull { get; }

        /// <summary>
        /// Type of the CSV column (field)
        /// </summary>
        internal Type? Type { get; }

        /// <summary>
        /// Value of the CSV column (field)
        /// </summary>
        internal object? Value { get; }
    }
}