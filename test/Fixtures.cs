using readerFlu;
using readerFlu.test.Models;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Unicode;
using System.Xml.Linq;

namespace readerFlu.test
{
    [TestClass]
    public class Fixtures
    {
        // ToDo: Mismatch Property Def and Class
        // ToDo: Support Basic Types

        /// <summary>
        /// a,b,c
        /// 1,2,3
        /// </summary>
        [TestMethod]
        public void Basic()
        {
            var path = "../../../fixtures/basic.csv";
            Assert.IsTrue(File.Exists(path));

            // Try a view different property types
            var parser = new CsvParser<Basic>();
            parser.Property<string>(c => c.A).ColumnName("a");
            parser.Property<int>(c => c.B).ColumnName("b");
            parser.Property<decimal>(c => c.C).ColumnName("c");
            List<Basic> result = parser.Parse(path, separator: ',');

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].A == "1");
            Assert.IsTrue(result[0].B == 2);
            Assert.IsTrue(result[0].C == 3);
        }

        /// <summary>
        /// # comment
        /// a,b,c
        /// 1,2,3
        /// </summary>
        [TestMethod]
        public void Comment()
        {
            var path = "../../../fixtures/comment.csv";
            Assert.IsTrue(File.Exists(path));

            // Try a different column order
            var parser = new CsvParser<Basic>();
            parser.Property<decimal>(c => c.C).ColumnName("c");
            parser.Property<int>(c => c.B).ColumnName("b");
            parser.Property<string>(c => c.A).ColumnName("a");
            List<Basic> result = parser.Parse(path, separator: ',');

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].C == 3);
            Assert.IsTrue(result[0].B == 2);
            Assert.IsTrue(result[0].A == "1");
        }

        [TestMethod]
        public void backtick()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/backtick.csv"));
        }

        [TestMethod]
        public void baddata()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/bad-data.csv"));
        }

        [TestMethod]
        public void CommaInQuote()
        {
            var path = "../../../fixtures/comma-in-quote.csv";
            Assert.IsTrue(File.Exists(path));
        }

        [TestMethod]
        public void emptycolumns()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/empty-columns.csv"));
        }

        [TestMethod]
        public void escapequotes()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/escape-quotes.csv"));
        }

        [TestMethod]
        public void geojson()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/geojson.csv"));
        }

        [TestMethod]
        public void headers()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/headers.csv"));
        }

        [TestMethod]
        public void largedataset()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/large-dataset.csv"));
        }

        [TestMethod]
        public void latin()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/latin.csv"));
        }

        [TestMethod]
        public void macnewlines()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/mac-newlines.csv"));
        }

        [TestMethod]
        public void newlines()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/newlines.csv"));
        }

        [TestMethod]
        public void noheaders()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/no-headers.csv"));
        }

        [TestMethod]
        public void optioncomment()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/option-comment.csv"));
        }

        [TestMethod]
        public void optionescape()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/option-escape.csv"));
        }

        [TestMethod]
        public void optionmaxRowBytes()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/option-maxRowBytes.csv"));
        }

        [TestMethod]
        public void optionnewline()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/option-newline.csv"));
        }

        [TestMethod]
        public void optionquote()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/option-quote.csv"));
        }

        [TestMethod]
        public void optionquoteescape()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/option-quote-escape.csv"));
        }

        [TestMethod]
        public void optionquotemany()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/option-quote-many.csv"));
        }

        [TestMethod]
        public void quotesnewlines()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/quotes+newlines.csv"));
        }

        [TestMethod]
        public void strict()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/strict.csv"));
        }

        [TestMethod]
        public void strictFalseLessColumns()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/strict-false-less-columns.csv"));
        }

        [TestMethod]
        public void strictfalsemorecolumns()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/strict-false-more-columns.csv"));
        }

        [TestMethod]
        public void strictskipLines()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/strict+skipLines.csv"));
        }

        #region File Formats

        [TestMethod]
        public void UTF16()
        {
            var path = "../../../fixtures/utf16.csv";
            Assert.IsTrue(File.Exists(path));

            var result = BasicStringParser().Parse(path, separator: ',', encoding: Encoding.Unicode);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);

            var utf8Text = Encoding.Unicode.GetString(new byte[] { 0xA4, 0x02 });
            Assert.IsTrue(result[1].C == utf8Text);
        }

        [TestMethod]
        public void UTF16Big()
        {
            var path = "../../../fixtures/utf16-big.csv";
            Assert.IsTrue(File.Exists(path));

            var result = BasicStringParser().Parse(path, separator: ',', encoding: Encoding.BigEndianUnicode);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);

            var utf8Text = Encoding.BigEndianUnicode.GetString(new byte[] { 0x02, 0xA4 });
            Assert.IsTrue(result[1].C == utf8Text);
        }

        [TestMethod]
        public void UTF8()
        {
            var path = "../../../fixtures/utf8.csv";
            Assert.IsTrue(File.Exists(path));

            var result = BasicStringParser().Parse(path, separator: ',', encoding: Encoding.UTF8);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);

            var utf8Text = Encoding.UTF8.GetString(new byte[] { 0xCA, 0xA4 });
            Assert.IsTrue(result[1].C == utf8Text);
        }

        #endregion File Formats

        #region Private

        private static CsvParser<BasicString> BasicStringParser()
        {
            var parser = new CsvParser<BasicString>();
            parser.Property<string>(c => c.A).ColumnName("a");
            parser.Property<string>(c => c.C).ColumnName("c");
            parser.Property<string>(c => c.B).ColumnName("b");
            return parser;
        }

        #endregion Private
    }
}