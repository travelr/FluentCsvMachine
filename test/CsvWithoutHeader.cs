using FluentCsvMachine.Test.Models;

namespace FluentCsvMachine.Test
{
    [TestClass]
    public class CsvWithoutHeader
    {
        /// <summary>
        /// a,b,c
        /// 1,2,3
        /// 4,5,6
        /// 7,8,9,10
        /// </summary>
        [TestMethod]
        public async Task Basic()
        {
            const string path = "../../../fixtures/no-headers.csv";
            Assert.IsTrue(File.Exists(path));

            // .C has no Attribute, therefore will no be mapped
            // 10 cannot be accessed because the previous lines do not offer an empty field

            var parser = new CsvParser<BasicString>();
            var result = await parser.ParseWithoutHeader(path, new CsvConfiguration());

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 4);
            Assert.IsTrue(result[0].A == "a" && result[0].B == "b" && result[0].D == "c");
            Assert.IsTrue(result[1].A == "1" && result[1].B == "2" && result[1].D == "3");
            Assert.IsTrue(result[2].A == "4" && result[2].B == "5" && result[2].D == "6");
            Assert.IsTrue(result[3].A == "7" && result[3].B == "8" && result[3].D == "9");
        }

        /// <summary>
        /// ,somejunk,<! />
        /// ,nope,
        /// yes,yup,yeah
        /// ok, ok, ok!
        /// </summary>
        [TestMethod]
        public async Task BadData()
        {
            const string path = "../../../fixtures/bad-data.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<BasicString>();
            var result = await parser.ParseWithoutHeader(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 4);
            Assert.IsTrue(result[0].A == null && result[0].B == "somejunk" && result[0].D == "<! />");
            Assert.IsTrue(result[1].A == null && result[1].B == "nope" && result[1].D == null);
            Assert.IsTrue(result[2].A == "yes" && result[2].B == "yup" && result[2].D == "yeah");
            Assert.IsTrue(result[3].A == "ok" && result[3].B == "ok" && result[3].D == "ok!");
        }

        [TestMethod]
        public async Task EmptyColumns()
        {
            const string path = "../../../fixtures/empty-columns.csv";
            Assert.IsTrue(File.Exists(path));

            var parser = new CsvParser<EmptyColumn>();
            var result = await parser.ParseWithoutHeader(path, new CsvConfiguration(';'));

            Assert.IsTrue(result[0].A == new DateTime(2007, 01, 01) && result[0].B == null && result[0].C == null);
            Assert.IsTrue(result[1].A == new DateTime(2007, 01, 02) && result[1].B == null && result[1].C == null);
        }
    }
}