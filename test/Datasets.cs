using Microsoft.VisualBasic;
using FluentCsvMachine;
using FluentCsvMachine.Helpers;
using FluentCsvMachine.Test.Models;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Unicode;
using System.Xml.Linq;

namespace FluentCsvMachine.Test
{
    [TestClass]
    public class Datasets
    {
        /// <summary>
        /// a,b,c
        /// 1,2,3
        /// </summary>
        [TestMethod]
        public void Tiny()
        {
            var path = "../../../fixtures/big-tiny.csv";
            Assert.IsTrue(File.Exists(path));

            var result = GetParser().Parse(path, new CsvConfiguration(','));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 25);

            Assert.IsTrue(result[0].Longitude == 59.53284d, "double , . issue");
            Assert.IsTrue(result[0].String == "g4Q%)V");
        }

        [TestMethod]
        public void Huge750K()
        {
            var path = "../../../fixtures/big-750k.csv";
            Assert.IsTrue(File.Exists(path));

            var result = GetParser().Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 750000);

            Assert.IsTrue(result[30].Email == "ed@be.edu");
            Assert.IsTrue(result[30].Pick == LargeDataSetPick.RED);
            Assert.IsTrue(result[30].Date == new DateTime(1974, 5, 2));
            Assert.IsTrue(result[651477].First == "Martha");
            Assert.IsTrue(result[651477].Age == 32);
            Assert.IsTrue(result[651477].Zip == 90461);
        }

        private static CsvParser<LargeDataSet> GetParser()
        {
            // email,first,last,age,street,city,state,zip,digid,date(2012/11/25),latitude,longitude,pick(RED|BLUE|YELLOW|GREEN|WHITE),string
            var parser = new CsvParser<LargeDataSet>();
            parser.Property<string>(c => c.Email).ColumnName("email");
            parser.Property<string>(c => c.First).ColumnName("first");
            parser.Property<string>(c => c.Last).ColumnName("last");
            parser.Property<int>(c => c.Age).ColumnName("age");
            parser.Property<string>(c => c.Street).ColumnName("street");
            parser.Property<string>(c => c.City).ColumnName("city");
            parser.Property<string>(c => c.State).ColumnName("state");
            parser.Property<int>(c => c.Zip).ColumnName("zip");
            parser.Property<DateTime>(c => c.Date).ColumnName("date").InputFormat("yyyy/MM/dd");
            parser.Property<double>(c => c.Latitude).ColumnName("latitude");
            parser.Property<double>(c => c.Longitude).ColumnName("longitude");
            parser.CustomMappingColumn((obj, value) =>
            {
                if (value == null || obj is not LargeDataSet d)
                {
                    return;
                }
                d.Pick = EnumHelpers<LargeDataSetPick>.Convert(value);
            }).ColumnName("pick");
            parser.Property<string>(c => c.String).ColumnName("string");
            return parser;
        }
    }
}