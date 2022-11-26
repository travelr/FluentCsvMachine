using FluentCsvMachine.Property;
using FluentCsvMachine.Test.Models;

namespace FluentCsvMachine.Test
{
    [TestClass]
    public class DataSets
    {
        [TestMethod]
        public void Orders()
        {
            const string path = "../../../fixtures/orders.csv";
            Assert.IsTrue(File.Exists(path));

            //Region,Country,Item Type,Sales Channel,Order Priority,Order Date,Order ID,Ship Date,Units Sold,Unit Price,Unit Cost,Total Revenue,Total Cost,Total Profit

            var parser = new CsvParser<OrdersDs>();
            parser.Property<OrdersDsRegion>(c => c.Region).ColumnName("Region");
            parser.Property<OrdersDsCountry>(c => c.Country).ColumnName("Country");
            parser.Property<OrdersDsItemType>(c => c.ItemType).ColumnName("Item Type");
            parser.Property<OrdersDsSalesChannel>(c => c.SalesChannel).ColumnName("Sales Channel");
            parser.Property<char>(c => c.OrderPriority).ColumnName("Order Priority");
            parser.Property<DateTime>(c => c.OrderDate).ColumnName("Order Date").InputFormat("MM/dd/yyyy");
            parser.Property<long>(c => c.OrderId).ColumnName("Order ID");
            parser.Property<DateTime>(c => c.ShipDate).ColumnName("Ship Date").InputFormat("MM/dd/yyyy");
            parser.Property<int>(c => c.UnitsSold).ColumnName("Units Sold");
            parser.Property<decimal>(c => c.UnitPrice).ColumnName("Unit Price");
            parser.Property<decimal>(c => c.UnitCost).ColumnName("Unit Cost");
            parser.Property<decimal>(c => c.TotalRevenue).ColumnName("Total Revenue");
            parser.Property<decimal>(c => c.TotalCost).ColumnName("Total Cost");
            parser.Property<decimal>(c => c.TotalProfit).ColumnName("Total Profit");

            var result = parser.Parse(path, new CsvConfiguration(','));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 65535);
            Assert.IsTrue(result[65534].Region == OrdersDsRegion.MiddleEastandNorthAfrica);
            Assert.IsTrue(result[65534].Country == OrdersDsCountry.Israel);
            Assert.IsTrue(result[65534].ItemType == OrdersDsItemType.Cereal);
            Assert.IsTrue(result[65534].SalesChannel == OrdersDsSalesChannel.Online);
            Assert.IsTrue(result[65534].OrderPriority == 'H');
            Assert.IsTrue(result[65534].OrderDate == new DateTime(2011, 8, 5));
            Assert.IsTrue(result[65534].OrderId == 253194797L);
            Assert.IsTrue(result[65534].ShipDate == new DateTime(2011, 8, 28));
            Assert.IsTrue(result[65534].UnitsSold == 4822);
            Assert.IsTrue(result[65534].UnitPrice == 205.7m);
            Assert.IsTrue(result[65534].UnitCost == 117.11m);
            Assert.IsTrue(result[65534].TotalRevenue == 991885.4m);
            Assert.IsTrue(result[65534].TotalCost == 564704.42m);
            Assert.IsTrue(result[65534].TotalProfit == 427180.98m);
        }


        [TestMethod]
        public void Tiny()
        {
            const string path = "../../../fixtures/big-tiny.csv";
            Assert.IsTrue(File.Exists(path));

            var result = GetParser().Parse(path, new CsvConfiguration(','));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 25);

            Assert.IsTrue(result[0].Longitude == 59.53284d);
            Assert.IsTrue(result[0].String == "g4Q%)V");
        }

        [TestMethod]
        public void Huge750K()
        {
            const string path = "../../../fixtures/big-750k.csv";
            Assert.IsTrue(File.Exists(path));

            var result = GetParser().Parse(path);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 750000);

            Assert.IsTrue(result[30].Email == "ed@be.edu");
            Assert.IsTrue(result[30].Pick == BigDataSetPick.RED);
            Assert.IsTrue(result[30].Date == new DateTime(1974, 5, 2));
            Assert.IsTrue(result[651477].First == "Martha");
            Assert.IsTrue(result[651477].Age == 32);
            Assert.IsTrue(result[651477].Zip == 90461);
        }

        private static CsvParser<BigDataSet> GetParser()
        {
            // email,first,last,age,street,city,state,zip,digid,date(2012/11/25),latitude,longitude,pick(RED|BLUE|YELLOW|GREEN|WHITE),string
            var parser = new CsvParser<BigDataSet>();
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
            parser.Property<BigDataSetPick>(c => c.Pick).ColumnName("pick");
            parser.Property<string>(c => c.String).ColumnName("string");
            return parser;
        }
    }
}