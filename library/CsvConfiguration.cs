using FluentCsvMachine.Helpers;
using System.Text;

namespace FluentCsvMachine
{
    public class CsvConfiguration
    {
        public CsvConfiguration()
        { }

        public CsvConfiguration(char delimiter)
        {
            Delimiter = delimiter;
        }

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
        /// Comment character, not defined in RFC 4180
        /// </summary>
        public char? Comment { get; set; } = null;

        /// <summary>
        /// Char to separate number and fraction e.g. 3.33
        /// </summary>
        public char FloatingPoint { get; set; } = '.';

        #region Library parameters

        /// <summary>
        /// File read buffer size
        /// https://referencesource.microsoft.com/#mscorlib/system/io/stream.cs,2a0f078c2e0c0aa8,references
        /// </summary>
        public int BufferSize { get; set; } = 81920;

        /// <summary>
        /// Header needs to be defined in the first x lines
        /// </summary>
        public int HeaderSearchLimit { get; set; } = 10;

        #endregion Library parameters
    }
}