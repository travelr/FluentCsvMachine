using FluentCsvMachine.Test.Models;

namespace FluentCsvMachine.Test
{
    [TestClass]
    public class CsvWithoutHeader
    {
        [TestMethod]
        [Ignore("Test case not implemented yet")]
        public void Basic()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/no-headers.csv"));
        }

        [TestMethod]
        [Ignore("Test case not implemented yet")]
        public void Baddata()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/bad-data.csv"));
        }

        [TestMethod]
        [Ignore("Test case not implemented yet")]
        public void Emptycolumns()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/empty-columns.csv"));
        }


        /// <summary>
        /// a,b,c
        /// 1,2,3
        /// 4,5,6
        /// 7,8,9,10
        /// </summary>
        //[TestMethod]
        //public void NoHeaders()
        //{
        //    const string path = "../../../fixtures/no-headers.csv";
        //    Assert.IsTrue(File.Exists(path));

        //    var parser = new CsvParser<BasicInt>();
        //    parser.Property<int>(c => c.A).ColumnName("a");
        //    parser.Property<int>(c => c.B).ColumnName("b");
        //    parser.Property<int>(c => c.C).ColumnName("c");
        //    var result = parser.Parse(path);

        //    Assert.IsNotNull(result);
        //    Assert.IsTrue(result.Count == 3);
        //    Assert.IsTrue(result[2].A == 7);
        //    Assert.IsTrue(result[2].B == 8);
        //    Assert.IsTrue(result[2].C == 9);
        //}
    }
}