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
        private static readonly Dictionary<Type, ValueParser> valueParsers = new()
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

        private static readonly Dictionary<string, ValueParser> dateParsers = new();

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

            if (type.IsEnum && !valueParsers.ContainsKey(type))
            {
                var enumMachine = (ValueParser?)Activator.CreateInstance(typeof(EnumParser<>).MakeGenericType(type), type);
                if (enumMachine == null)
                {
                    throw new InvalidOperationException($"Couldn't create EnumParser<> of type {type}");
                }
                valueParsers[type] = enumMachine;
            }
            else if (type == typeof(DateTime))
            {
                if (inputFormat == null || inputFormat == string.Empty)
                {
                    ThrowHelper.ThrowCsvConfigurationException("Each DateTime column requires InputFormat()");
                }

                if (!dateParsers.TryGetValue(inputFormat, out var dateParser))
                {
                    dateParser = new DateTimeParser(inputFormat);
                    dateParsers.Add(inputFormat, dateParser);
                }

                return dateParser;
            }

            if (!valueParsers.TryGetValue(type, out ValueParser? returnValue))
            {
                throw new KeyNotFoundException($"Unknown ValueParser type {type}! Please create a bug ticket!");
            }

            return returnValue;
        }
    }
}