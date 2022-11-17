namespace FluentCsvMachine.Test
{
    [TestClass]
    public class CsvWithoutHeader
    {
        [TestMethod]
        [Ignore("Test case not implemented yet")]
        public void Basic()
        {
            Assert.IsTrue(File.Exists("../../../fixtures/bad-data.csv"));
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
    }
}