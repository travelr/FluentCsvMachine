using FluentCsvMachine.Helpers;
using System.Text;

namespace FluentCsvMachine
{
    /// <summary>
    /// CSV configuration
    /// Set the delimiter or other options
    /// All properties offer defaults, creating new() is fine
    /// </summary>
    public class CsvConfiguration
    {
        private char _decimalPoint = '.';

        /// <summary>
        /// CSV default configuration
        /// </summary>
        public CsvConfiguration()
        { }

        /// <summary>
        /// CSV default configuration with a custom delimiter
        /// </summary>
        public CsvConfiguration(char delimiter)
        {
            Delimiter = delimiter;
        }

        /// <summary>
        /// CSV default configuration with a custom delimiter and file encoding
        /// </summary>
        public CsvConfiguration(char delimiter, Encoding encoding) : this(delimiter)
        {
            Guard.IsNotNull(encoding);
            Encoding = encoding;
        }

        /// <summary>
        /// Delimiter separating columns
        /// </summary>
        public char Delimiter { get; set; } = ',';

        /// <summary>
        /// Encoding of the file
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.Latin1;

        /// <summary>
        /// New line character
        /// </summary>
        public char NewLine { get; set; } = '\n';

        /// <summary>
        /// Quote character, RFC 4180 only allows "
        /// </summary>
        public char Quote { get; set; } = '"';

        /// <summary>
        /// Quote escape character
        /// </summary>
        public char QuoteEscape { get; set; } = '"';

        /// <summary>
        /// Comment character, not defined in RFC 4180
        /// </summary>
        public char? Comment { get; set; } = null;

        /// <summary>
        /// Char to separate number and fraction e.g. 3.33
        /// </summary>
        public char DecimalPoint
        {
            get => _decimalPoint;
            set
            {
                if (value is not ('.' or ','))
                {
                    ThrowHelper.ThrowCsvConfigurationException("Decimal point only allows dot or comma as a value");
                }

                _decimalPoint = value;
                ThousandsChar = value == '.' ? ',' : '.';
            }
        }

        /// <summary>
        /// Char to separate thousands
        /// Set by DecimalPoint;
        /// </summary>
        public char ThousandsChar { get; private set; } = '.';

        #region Library parameters

        /// <summary>
        /// File read buffer size
        /// https://referencesource.microsoft.com/#mscorlib/system/io/stream.cs,2a0f078c2e0c0aa8,references
        /// </summary>
        public int BufferSize { get; set; } = 81920;

        /// <summary>
        /// Header needs to be defined in the first x lines
        /// </summary>
        public int HeaderSearchLimit { get; set; } = 15;

        /// <summary>
        /// Maximum number of columns
        /// Value is used for performance optimizations
        /// </summary>
        public int MaxNumberOfColumns { get; set; } = 15;

        #endregion Library parameters
    }
}