using System.Reflection.PortableExecutable;
using System.Text.Unicode;
using System.Xml.Linq;

namespace test
{
    [TestClass]
    public class Fixtures
    {
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
        public void basic()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/basic.csv"));
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
        public void utf16big()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/utf16-big.csv"));
        }

        [TestMethod]
        public void utf16()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/utf16.csv"));
        }

        [TestMethod]
        public void utf8()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/utf8.csv"));
        }

        [TestMethod]
        public void commainquote()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/comma-in-quote.csv"));
        }

        [TestMethod]
        public void comment()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/comment.csv"));
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
        public void optionquote()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/option-quote.csv"));
        }

        [TestMethod]
        public void quotesnewlines()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/quotes+newlines.csv"));
        }

        [TestMethod]
        public void strictskipLines()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/strict+skipLines.csv"));
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
        public void strict()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/strict.csv"));
        }
    }
}