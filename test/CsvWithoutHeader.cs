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
        public void Basic()
        {
            const string path = "../../../fixtures/no-headers.csv";
            Assert.IsTrue(File.Exists(path));

            // .C has no Attribute, therefore will no be mapped
            // 10 cannot be accessed because the previous lines do not offer an empty field

            var parser = new CsvParser<BasicString>();
            var result = parser.ParseWithoutHeader(path, new CsvConfiguration());

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 4);
            Assert.IsTrue(result[0].A == "a" && result[0].B == "b" && result[0].D == "c");
            Assert.IsTrue(result[1].A == "1" && result[1].B == "2" && result[1].D == "3");
            Assert.IsTrue(result[2].A == "4" && result[2].B == "5" && result[2].D == "6");
            Assert.IsTrue(result[3].A == "7" && result[3].B == "8" && result[3].D == "9");
        }

        [TestMethod]
        [Ignore("Test case not implemented yet")]
        public void BadData()
        {
            const string path = "../../../fixtures/bad-data.csv";
            Assert.IsTrue(File.Exists(path));
        }

        [TestMethod]
        [Ignore("Test case not implemented yet")]
        public void EmptyColumns()
        {
            const string path = "../../../fixtures/empty-columns.csv";
            Assert.IsTrue(File.Exists(path));
        }
    }
}