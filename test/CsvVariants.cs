using FluentCsvMachine;
using FluentCsvMachine.Test.Models;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Unicode;
using System.Xml.Linq;

namespace FluentCsvMachine.Test
{
    [TestClass]
    public class CsvVariants
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
            List<Basic> result = parser.Parse(path);

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
            List<Basic> result = parser.Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].C == 3);
            Assert.IsTrue(result[0].B == 2);
            Assert.IsTrue(result[0].A == "1");
        }

        /// <summary>
        /// pokemon_id`p_desc
        /// 1`Bulbasaur can be seen napping
        /// 2`There is a bud on this
        /// </summary>
        [TestMethod]
        public void BackTick()
        {
            var path = "../../../fixtures/backtick.csv";
            Assert.IsTrue(File.Exists(path));

            // Not using all properties
            var parser = new CsvParser<Basic>();
            parser.Property<int>(c => c.B).ColumnName("pokemon_id");
            parser.Property<string>(c => c.A).ColumnName("p_desc");

            List<Basic> result = parser.Parse(path, new CsvConfiguration('`'));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[0].B == 1);
            Assert.IsTrue(result[0].A == "Bulbasaur can be seen napping");
            Assert.IsTrue(result[1].B == 2);
            Assert.IsTrue(result[1].A == "There is a bud on this");
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
        public void mean()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/mean.csv"));
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/Comma-separated_values#Example
        /// </summary>
        [TestMethod]
        public void wiki()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/wiki.csv"));
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

        /// <summary>
        /// a,b,c
        /// 1,2,3
        /// 4,5
        /// 6,7,8
        /// </summary>
        [TestMethod]
        public void StrictFalseLessColumns()
        {
            var path = "../../../fixtures/strict-false-less-columns.csv";
            Assert.IsTrue(File.Exists(path));

            var result = BasicIntParser().Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);
        }

        /// <summary>
        /// a,b,c
        /// 1,2,3
        /// 4,5,6,7
        /// 8,9,10
        /// </summary>
        [TestMethod]
        public void StrictFalseMoreColumns()
        {
            var path = "../../../fixtures/strict-false-more-columns.csv";
            Assert.IsTrue(File.Exists(path));

            var result = BasicIntParser().Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);

            Assert.IsTrue(result[0].A == 1);
            Assert.IsTrue(result[1].C == 6);
            Assert.IsTrue(result[2].B == 9);
            Assert.IsTrue(result[2].C == 10);
        }

        /// <summary>
        /// a,b,c
        /// h1,h2,h3
        /// 1,2,3
        /// 4,5,6
        /// 7,8,9
        /// </summary>
        [TestMethod]
        public void StrictSkipLines()
        {
            var path = "../../../fixtures/strict+skipLines.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<BasicInt>();
            parser.Property<int>(c => c.A).ColumnName("h1");
            parser.Property<int>(c => c.B).ColumnName("h2");
            parser.Property<int>(c => c.C).ColumnName("h3");

            var result = parser.Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);

            Assert.IsTrue(result[0].A == 1);
            Assert.IsTrue(result[1].C == 6);
            Assert.IsTrue(result[2].B == 8);
            Assert.IsTrue(result[2].C == 9);
        }

        #region File Formats

        [TestMethod]
        public void UTF16()
        {
            var path = "../../../fixtures/utf16.csv";
            Assert.IsTrue(File.Exists(path));

            var result = BasicStringParser().Parse(path, new CsvConfiguration(',', Encoding.Unicode));

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

            var result = BasicStringParser().Parse(path, new CsvConfiguration(',', Encoding.BigEndianUnicode));

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

            var result = BasicStringParser().Parse(path, new CsvConfiguration(',', Encoding.UTF8));

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
            parser.Property<string>(c => c.B).ColumnName("b");
            parser.Property<string>(c => c.C).ColumnName("c");
            return parser;
        }

        private static CsvParser<BasicInt> BasicIntParser()
        {
            var parser = new CsvParser<BasicInt>();
            parser.Property<int>(c => c.A).ColumnName("a");
            parser.Property<int>(c => c.B).ColumnName("b");
            parser.Property<int>(c => c.C).ColumnName("c");
            return parser;
        }

        #endregion Private
    }
}