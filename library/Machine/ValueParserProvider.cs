using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Values;

namespace FluentCsvMachine.Machine
{
    /// <summary>
    /// Static class to provide ValueParsers
    /// Make sure that you assign ValueParser.Config
    /// </summary>
    internal static class ValueParserProvider
    {
        private static readonly Dictionary<Type, ValueParser> ValueParsers = new()
        {
            { typeof(string),  new StringParser() },
            { typeof(byte),  new BinaryIntegerParser<byte>() },
            { typeof(short),  new BinaryIntegerParser<short>() },
            { typeof(int),  new BinaryIntegerParser<int>() },
            { typeof(long),  new BinaryIntegerParser<long>() },
            { typeof(float),  new FloatingPointParser<float>() },
            { typeof(double),  new FloatingPointParser<double>() },
            { typeof(decimal),  new FloatingPointParser<decimal>() },
        };

        private static readonly Dictionary<string, ValueParser> DateParsers = new();

        /// <summary>
        /// Gets the correct parser based on the passed type
        /// </summary>
        /// <param name="type">Type of the column (field)</param>
        /// <param name="inputFormat">Mandatory for DateTime types</param>
        /// <returns>ValueParser for the type</returns>
        /// <exception cref="InvalidOperationException">Creation of an Enum parser failed</exception>
        /// <exception cref="KeyNotFoundException">Parser does not exist</exception>
        internal static ValueParser GetParser(Type type, string? inputFormat = null)
        {
            Guard.IsNotNull(type);

            if (type.IsEnum && !ValueParsers.ContainsKey(type))
            {
                var enumMachine = (ValueParser?)Activator.CreateInstance(typeof(EnumParser<>).MakeGenericType(type), type);
                ValueParsers[type] = enumMachine ?? throw new InvalidOperationException($"Couldn't create EnumParser<> of type {type}");
            }
            else if (type == typeof(DateTime))
            {
                if (string.IsNullOrEmpty(inputFormat))
                {
                    ThrowHelper.ThrowCsvConfigurationException("Each DateTime column requires InputFormat()");
                }

                if (!DateParsers.TryGetValue(inputFormat, out var dateParser))
                {
                    dateParser = new DateTimeParser(inputFormat);
                    DateParsers.Add(inputFormat, dateParser);
                }

                return dateParser;
            }

            if (!ValueParsers.TryGetValue(type, out ValueParser? returnValue))
            {
                throw new KeyNotFoundException($"Unknown ValueParser type {type}! Please create a bug ticket!");
            }

            return returnValue;
        }
    }
}