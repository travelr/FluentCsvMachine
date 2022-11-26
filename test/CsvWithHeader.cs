using FluentCsvMachine.Property;
using FluentCsvMachine.Test.Models;
using System.Text;

// ReSharper disable CommentTypo
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace FluentCsvMachine.Test
{
    [TestClass]
    public class CsvWithHeader
    {
        // ToDo: Configuration tests (same col name, same setter, ...)


        /// <summary>
        /// a,b,c
        /// 1,2,3
        /// </summary>
        [TestMethod]
        public void Basic()
        {
            const string path = "../../../fixtures/basic.csv";
            Assert.IsTrue(File.Exists(path));

            // Try a view different property types
            var parser = new CsvParser<Basic>();
            parser.Property<string>(c => c.A).ColumnName("a");
            parser.Property<int>(c => c.B).ColumnName("b");
            parser.Property<decimal?>(c => c.C).ColumnName("c");
            var result = parser.Parse(path);

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
            const string path = "../../../fixtures/comment.csv";
            Assert.IsTrue(File.Exists(path));

            // Try a different column order
            var parser = new CsvParser<Basic>();
            parser.Property<decimal?>(c => c.C).ColumnName("c");
            parser.Property<int>(c => c.B).ColumnName("b");
            parser.Property<string>(c => c.A).ColumnName("a");

            var result = parser.Parse(path, new CsvConfiguration() { Comment = '#' });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].A == "1");
            Assert.IsTrue(result[0].B == 2222);
            Assert.IsTrue(result[0].C == 33333.33m);
        }

        /// <summary>
        /// pokemon_id`p_desc
        /// 1`Bulbasaur can be seen napping
        /// 2`There is a bud on this
        /// --
        /// Try a few advanced features of this library
        /// </summary>
        [TestMethod]
        public void BackTick()
        {
            const string path = "../../../fixtures/backtick.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<Basic>();
            // Different order between CSV columns and properties are no problem
            parser.Property<int>(c => c.B).ColumnName("pokemon_id");

            // Lets use a PropertyCustom and an inefficient LineAction 
            parser.PropertyCustom<string>((entity, value) =>
                {
                    // Make string lower case
                    entity.A = value.ToLowerInvariant();
                })
                .ColumnName("p_desc");

            // Executed after entity creation
            parser.LineAction((x, l) =>
            {
                // Make sure that the first char is lowercase -> PropertyCustom
                Assert.IsTrue(char.IsLower(x.A![0]));

                // Revert the PropertyCustom just to explain the order
                var csvFieldBeforeCustomAction = l[1]!.ToString(); // Not SAFE!
                x.A = csvFieldBeforeCustomAction;
            });

            var result = parser.Parse(path, new CsvConfiguration('`'));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[0].B == 1);
            Assert.IsTrue(result[0].A == "Bulbasaur can be seen napping");
            Assert.IsTrue(result[1].B == 2);
            Assert.IsTrue(result[1].A == "There is a bud on this");
        }

        /// <summary>
        /// first,last,address,city,zip
        /// John,Doe,120 any st.,"Anytown, WW",08123
        /// </summary>
        [TestMethod]
        public void CommaInQuote()
        {
            const string path = "../../../fixtures/comma-in-quote.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<AddressData>();
            parser.Property<string>(c => c.FirstName).ColumnName("first");
            parser.Property<string>(c => c.LastName).ColumnName("last");
            parser.Property<string>(c => c.Address).ColumnName("address");
            parser.Property<string>(c => c.City).ColumnName("city");
            parser.Property<string>(c => c.Zip).ColumnName("zip");

            var result = parser.Parse(path);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].City == "Anytown, WW");
            Assert.IsTrue(result[0].Zip == "08123");
        }

        /// <summary>
        /// a,b
        /// 1,"ha ""ha"" ha"
        /// 2,""""""
        /// 3,4
        /// </summary>
        [TestMethod]
        public void EscapeQuotes()
        {
            const string path = "../../../fixtures/escape-quotes.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<BasicString>();
            parser.Property<string>(c => c.A).ColumnName("a");
            parser.Property<string>(c => c.B).ColumnName("b");

            var result = parser.Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result[0].B == "ha \"ha\" ha");
            Assert.IsTrue(result[1].B == "\"\"");
            Assert.IsTrue(result[2].B == "4");
        }

        [TestMethod]
        [Ignore("Test case not implemented yet")]
        public void Mean()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/mean.csv"));
        }

        /// <summary>
        /// https://en.wikipedia.org/wiki/Comma-separated_values#Example
        /// Year,Make,Model,Description,Price
        /// 1997,Ford,E350,"ac, abs, moon",3000.00
        /// 1999,Chevy,"Venture ""Extended Edition""","",4900.00
        /// 1999,Chevy,"Venture ""Extended Edition, Very Large""","",5000.00
        /// 1996,Jeep,Grand Cherokee,"MUST SELL!
        /// air, moon roof, loaded",4799.00
        /// </summary>
        [TestMethod]
        [Ignore("Test case not implemented yet")]
        public void Wiki()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/wiki.csv"));
        }

        /// <summary>
        /// id,prop0,prop1,geojson
        /// ,value0,,"{""type"": ""Point"", ""coordinates"": [102.0, 0.5]}"
        /// ,value0,0.0,"{""type"": ""LineString"", ""coordinates"": [[102.0, 0.0], [103.0, 1.0], [104.0, 0.0], [105.0, 1.0]]}"
        /// ,value0,{u'this': u'that'},"{""type"": ""Polygon"", ""coordinates"": [[[100.0, 0.0], [101.0, 0.0], [101.0, 1.0], [100.0, 1.0], [100.0, 0.0]]]}"
        /// </summary>
        [TestMethod]
        public void GeoJson()
        {
            const string path = "../../../fixtures/geojson.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<BasicString>();
            parser.Property<string>(c => c.A).ColumnName("id");
            parser.Property<string>(c => c.B).ColumnName("prop0");
            parser.Property<string>(c => c.C).ColumnName("prop1");
            parser.Property<string>(c => c.D).ColumnName("geojson");

            var result = parser.Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result.All(x => x.A == null));
            Assert.IsTrue(result.All(x => x.B == "value0"));
            Assert.IsTrue(result[2].C == "{u'this': u'that'}");
            Assert.IsTrue(result[0].D == "{\"type\": \"Point\", \"coordinates\": [102.0, 0.5]}");
            Assert.IsTrue(result[2].D == "{\"type\": \"Polygon\", \"coordinates\": [[[100.0, 0.0], [101.0, 0.0], [101.0, 1.0], [100.0, 1.0], [100.0, 0.0]]]}");
        }

        [TestMethod]
        [Ignore("Test case not implemented yet")]
        public void Largedataset()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/large-dataset.csv"));
        }

        /// <summary>
        /// a,b,c
        /// 1,2,3
        /// "Once upon 
        /// a time",5,6
        /// </summary>
        /// <remarks>\r as line breaks</remarks>
        [TestMethod]
        public void MacNewlines()
        {
            const string path = "../../../fixtures/mac-newlines.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<BasicString>();
            parser.Property<string>(c => c.A).ColumnName("a");

            var result = parser.Parse(path, new CsvConfiguration() { NewLine = '\r' });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[1].A == "Once upon\ra time");
        }

        /// <summary>
        /// a,b,c
        /// 1,2,3
        /// "Once upon 
        /// a time",5,6
        /// 7,8,9
        /// </summary>
        [TestMethod]
        public void NewLines()
        {
            const string path = "../../../fixtures/newlines.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<BasicString>();
            parser.Property<string>(c => c.A).ColumnName("a");

            var result = parser.Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result[1].A == "Once upon \na time");
        }

        /// <summary>
        /// # Numbers and mean things ;;;;;;
        /// 
        /// # ; Empty Column at the end and in between
        /// 
        /// a;b;;c;d;
        /// 100;0;;200;300;
        /// -99;;;2,2;3,03;
        /// -4;-0,0;;123.456.789,123;-6,6666666;
        /// "-4";"0";"";"123.456.789,123";"-6,6666666";
        /// </summary>
        [TestMethod]
        public void Numbers()
        {
            const string path = "../../../fixtures/numbers.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<Numbers>();
            parser.Property<short>(c => c.A).ColumnName("a");
            parser.Property<float?>(c => c.B).ColumnName("b");
            parser.Property<decimal>(c => c.C).ColumnName("c");
            parser.Property<double>(c => c.D).ColumnName("d");
            var result = parser.Parse(path, new CsvConfiguration() { Delimiter = ';', DecimalPoint = ',' });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 4);
            Assert.IsTrue(result[0].A == 100 && result[0].B == 0 && result[0].C == 200m && result[0].D == 300d);
            Assert.IsTrue(result[1].A == -99 && !result[1].B.HasValue && result[1].C == 2.2m && result[1].D == 3.03d);
            Assert.IsTrue(result[2].A == -4 && result[2].B == 0 && result[2].C == 123456789.123m && result[2].D == -6.123456d);
            Assert.IsTrue(result[3].A == -4 && result[3].B == 0 && result[3].C == 123456789.123m && result[3].D == -6.123456d);
        }

        /// <summary>
        /// ~ comment
        /// a,b,c
        /// 1,2,3
        /// </summary>
        [TestMethod]
        public void OptionComment()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/option-comment.csv"));

            const string path = "../../../fixtures/option-comment.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = BasicIntParser();
            var result = parser.Parse(path, new CsvConfiguration() { Comment = '~' });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 1);
            Assert.IsTrue(result[0].A == 1);
            Assert.IsTrue(result[0].B == 2);
            Assert.IsTrue(result[0].C == 3);
        }

        /// <summary>
        /// a,b,c
        /// 1,"some \"escaped\" value",2
        /// 3,"\"\"",4
        /// 5,6,7
        /// </summary>
        [TestMethod]
        public void OptionEscape()
        {
            const string path = "../../../fixtures/option-escape.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<Basic2>();
            parser.Property<int?>(c => c.A).ColumnName("a");
            parser.Property<string>(c => c.B).ColumnName("b");
            parser.Property<int?>(c => c.C).ColumnName("c");

            var result = parser.Parse(path, new CsvConfiguration() { QuoteEscape = '\\' });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);

            Assert.IsTrue(result[0].B == "some \"escaped\" value");
            Assert.IsTrue(result[1].B == "\"\"");
            Assert.IsTrue(result[2].B == "6");
        }

        [TestMethod]
        [Ignore("Test case not implemented yet")]
        public void OptionMaxRowBytes()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/option-maxRowBytes.csv"));
        }

        /// <summary>
        /// a,b,cX1,2,3X"X-Men",5,6X7,8,9
        /// </summary>
        [TestMethod]
        public void OptionNewline()
        {
            const string path = "../../../fixtures/option-newline.csv";
            Assert.IsTrue(File.Exists(path));

            // X as \n
            var parser = new CsvParser<Basic>();
            parser.Property<string>(c => c.A).ColumnName("a");
            parser.Property<int>(c => c.B).ColumnName("b");
            parser.Property<decimal?>(c => c.C).ColumnName("c");

            var result = parser.Parse(path, new CsvConfiguration() { NewLine = 'X' });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result[1].A == "X-Men");
            Assert.IsTrue(result[1].B == 5);
            Assert.IsTrue(result[1].C == 6);
            Assert.IsTrue(result[2].B == 8);
            Assert.IsTrue(result[2].C == 9);
        }

        /// <summary>
        /// a,b,c
        /// 1,'some value',2
        /// 3,4,5
        /// </summary>
        [TestMethod]
        public void OptionQuote()
        {
            const string path = "../../../fixtures/option-quote.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<Basic2>();
            parser.Property<int?>(c => c.A).ColumnName("a");
            parser.Property<string>(c => c.B).ColumnName("b");
            parser.Property<int?>(c => c.C).ColumnName("c");
            CsvConfiguration config = new()
            {
                Quote = '\''
            };
            var result = parser.Parse(path, config);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[0].B == "some value");
            Assert.IsTrue(result[0].C == 2);
            Assert.IsTrue(result[1].B == "4");
            Assert.IsTrue(result[1].C == 5);
        }

        /// <summary>
        /// Escape Quote by a different char
        /// a,b,c
        /// 1,'some \'escaped\' value',2
        /// 3,'\'\'',4
        /// 5,6,7
        /// </summary>
        [TestMethod]
        public void OptionQuoteEscape()
        {
            const string path = "../../../fixtures/option-quote-escape.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<Basic2>();
            parser.Property<int?>(c => c.A).ColumnName("a");
            parser.Property<string>(c => c.B).ColumnName("b");
            parser.Property<int?>(c => c.C).ColumnName("c");

            var result = parser.Parse(path, new CsvConfiguration() { Quote = '\'', QuoteEscape = '\\' });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);

            Assert.IsTrue(result[0].B == "some 'escaped' value");
            Assert.IsTrue(result[1].B == "''");
            Assert.IsTrue(result[2].B == "6");
        }

        /// <summary>
        /// Escape Quote like defined in RFC
        /// a,b,c
        /// 1,'some ''escaped'' value',2
        /// 3,'''''',4
        /// 5,6,7
        /// </summary>
        [TestMethod]
        public void OptionQuoteMany()
        {
            const string path = "../../../fixtures/option-quote-many.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<Basic2>();
            parser.Property<int?>(c => c.A).ColumnName("a");
            parser.Property<string>(c => c.B).ColumnName("b");
            parser.Property<int?>(c => c.C).ColumnName("c");

            var result = parser.Parse(path, new CsvConfiguration() { Quote = '\'' });

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result[0].B == "some 'escaped' value");
            Assert.IsTrue(result[0].C == 2);
            Assert.IsTrue(result[1].B == "''");
            Assert.IsTrue(result[1].C == 4);
            Assert.IsTrue(result[2].B == "6");
            Assert.IsTrue(result[2].C == 7);
        }

        /// <summary>
        /// a,b
        /// 1,"ha
        /// ""ha""
        /// ha"
        /// 2,"
        /// """"
        /// "
        /// 3,4
        /// </summary>
        [TestMethod]
        public void QuotesNewlines()
        {
            const string path = "../../../fixtures/quotes+newlines.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<Basic2>();
            parser.Property<int?>(c => c.A).ColumnName("a");
            parser.Property<string>(c => c.B).ColumnName("b");

            var result = parser.Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(result[0].B == "ha \n\"ha\" \nha");
            Assert.IsTrue(result[1].B == " \n\"\" \n");
            Assert.IsTrue(result[2].A == 3);
            Assert.IsTrue(result[2].B == "4");
        }


        /// <summary>
        /// a,b,c
        /// 1,2,3
        /// 4,5
        /// 6,7,8
        /// </summary>
        [TestMethod]
        public void FalseLessColumns()
        {
            const string path = "../../../fixtures/strict-false-less-columns.csv";
            Assert.IsTrue(File.Exists(path));

            var result = BasicIntParser().Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);
            Assert.IsTrue(!result[1].C.HasValue);
        }

        /// <summary>
        /// a,b,c
        /// 1,2,3
        /// 4,5,6,7
        /// 8,9,10
        /// </summary>
        [TestMethod]
        public void FalseMoreColumns()
        {
            const string path = "../../../fixtures/strict-false-more-columns.csv";
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
        public void SkipLines()
        {
            const string path = "../../../fixtures/strict+skipLines.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<BasicInt>();
            parser.Property<int?>(c => c.A).ColumnName("h1");
            parser.Property<int?>(c => c.B).ColumnName("h2");
            parser.Property<int?>(c => c.C).ColumnName("h3");

            var result = parser.Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 3);

            Assert.IsTrue(result[0].A == 1);
            Assert.IsTrue(result[1].C == 6);
            Assert.IsTrue(result[2].B == 8);
            Assert.IsTrue(result[2].C == 9);
        }

        #region File Formats

        /// <summary>
        /// a,b,c
        /// 1,2,3
        /// 4,5,©
        /// </summary>
        [TestMethod]
        public void Latin()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/latin.csv"));

            const string path = "../../../fixtures/latin.csv";
            Assert.IsTrue(File.Exists(path));

            // Try a view different property types
            var parser = new CsvParser<BasicString>();
            parser.Property<string>(c => c.A).ColumnName("a");
            parser.Property<string>(c => c.B).ColumnName("b");
            parser.Property<string>(c => c.C).ColumnName("c");
            var result = parser.Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);
            Assert.IsTrue(result[1].C == Encoding.Latin1.GetString(new byte[] { 0xA9 }));
        }

        [TestMethod]
        public void Utf16()
        {
            const string path = "../../../fixtures/utf16.csv";
            Assert.IsTrue(File.Exists(path));

            var result = BasicStringParser().Parse(path, new CsvConfiguration(',', Encoding.Unicode));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);

            var utf8Text = Encoding.Unicode.GetString(new byte[] { 0xA4, 0x02 });
            Assert.IsTrue(result[1].C == utf8Text);
        }

        [TestMethod]
        public void Utf16Big()
        {
            const string path = "../../../fixtures/utf16-big.csv";
            Assert.IsTrue(File.Exists(path));

            var result = BasicStringParser().Parse(path, new CsvConfiguration(',', Encoding.BigEndianUnicode));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 2);

            var utf8Text = Encoding.BigEndianUnicode.GetString(new byte[] { 0x02, 0xA4 });
            Assert.IsTrue(result[1].C == utf8Text);
        }

        [TestMethod]
        public void Utf8()
        {
            const string path = "../../../fixtures/utf8.csv";
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
            parser.Property<string?>(c => c.A).ColumnName("a");
            parser.Property<string?>(c => c.B).ColumnName("b");
            parser.Property<string?>(c => c.C).ColumnName("c");
            return parser;
        }

        private static CsvParser<BasicInt> BasicIntParser()
        {
            var parser = new CsvParser<BasicInt>();
            parser.Property<int?>(c => c.A).ColumnName("a");
            parser.Property<int?>(c => c.B).ColumnName("b");
            parser.Property<int?>(c => c.C).ColumnName("c");
            return parser;
        }

        #endregion Private
    }
}