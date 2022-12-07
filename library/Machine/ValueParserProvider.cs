using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Values;

namespace FluentCsvMachine.Machine
{
    /// <summary>
    /// Provides ValueParsers, cannot be static due to parallel use
    /// Make sure that you assign ValueParser.Config -> WorkflowInput
    /// </summary>
    internal class ValueParserProvider
    {
        private readonly Dictionary<Type, ValueParser> _valueParsers = new()
        {
            { typeof(string), new StringParser() },
            { typeof(char), new CharParser(nullable: false) },
            { typeof(char?), new CharParser(nullable: true) },
            { typeof(byte), new BinaryIntegerParser<byte>(nullable: false, signed: false) },
            { typeof(byte?), new BinaryIntegerParser<byte>(nullable: true, signed: false) },
            { typeof(sbyte), new BinaryIntegerParser<sbyte>(nullable: false, signed: true) },
            { typeof(sbyte?), new BinaryIntegerParser<sbyte>(nullable: true, signed: true) },
            { typeof(short), new BinaryIntegerParser<short>(nullable: false, signed: true) },
            { typeof(short?), new BinaryIntegerParser<short>(nullable: true, signed: true) },
            { typeof(ushort), new BinaryIntegerParser<ushort>(nullable: false, signed: false) },
            { typeof(ushort?), new BinaryIntegerParser<ushort>(nullable: true, signed: false) },
            { typeof(int), new BinaryIntegerParser<int>(nullable: false, signed: true) },
            { typeof(int?), new BinaryIntegerParser<int>(nullable: true, signed: true) },
            { typeof(uint), new BinaryIntegerParser<uint>(nullable: false, signed: false) },
            { typeof(uint?), new BinaryIntegerParser<uint>(nullable: true, signed: false) },
            { typeof(long), new BinaryIntegerParser<long>(nullable: false, signed: true) },
            { typeof(long?), new BinaryIntegerParser<long>(nullable: true, signed: true) },
            { typeof(ulong), new BinaryIntegerParser<ulong>(nullable: false, signed: false) },
            { typeof(ulong?), new BinaryIntegerParser<ulong>(nullable: true, signed: false) },
            { typeof(float), new FloatingPointParser<float>(nullable: false) },
            { typeof(float?), new FloatingPointParser<float>(nullable: true) },
            { typeof(double), new FloatingPointParser<double>(nullable: false) },
            { typeof(double?), new FloatingPointParser<double>(nullable: true) },
            { typeof(decimal), new FloatingPointParser<decimal>(nullable: false) },
            { typeof(decimal?), new FloatingPointParser<decimal>(nullable: true) },
        };

        private readonly Dictionary<(string, bool), ValueParser> _dateParsers = new();


        /// <summary>
        /// Gets the correct parser based on the passed type
        /// </summary>
        /// <param name="type">Type of the column (field)</param>
        /// <param name="inputFormat">Mandatory for DateTime types</param>
        /// <returns>ValueParser for the type</returns>
        /// <exception cref="InvalidOperationException">Creation of an Enum parser failed</exception>
        /// <exception cref="KeyNotFoundException">Parser does not exist</exception>
        internal ValueParser GetParser(Type type, string? inputFormat = null)
        {
            Guard.IsNotNull(type);

            if (type.IsEnum && !_valueParsers.ContainsKey(type))
            {
                var enumMachine = (ValueParser?)Activator.CreateInstance(typeof(EnumParser<>).MakeGenericType(type), type);
                _valueParsers[type] = enumMachine ?? throw new InvalidOperationException($"Couldn't create EnumParser<> of type {type}");
            }
            else if (type == typeof(DateTime))
            {
                return GetDateParser(inputFormat, false);
            }
            else if (type == typeof(DateTime?))
            {
                return GetDateParser(inputFormat, true);
            }

            if (!_valueParsers.TryGetValue(type, out ValueParser? returnValue))
            {
                throw new KeyNotFoundException($"Unknown ValueParser type {type}! Please create a bug ticket!");
            }

            return returnValue;
        }

        private ValueParser GetDateParser(string? inputFormat, bool nullable)
        {
            if (string.IsNullOrEmpty(inputFormat))
            {
                ThrowHelper.ThrowCsvConfigurationException("Each DateTime column requires InputFormat()");
            }

            if (_dateParsers.TryGetValue((inputFormat, nullable), out var dateParser))
            {
                return dateParser;
            }

            dateParser = new DateTimeParser(inputFormat, nullable);
            _dateParsers.Add((inputFormat, nullable), dateParser);

            return dateParser;
        }
    }
}