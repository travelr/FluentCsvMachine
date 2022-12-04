using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using FluentCsvMachine.Test.Models;

namespace FluentCsvMachine.Benchmark
{
    [SimpleJob(RunStrategy.Monitoring, launchCount: 10, warmupCount: 0, targetCount: 10)]
    public class BigDataSetBenchmark
    {
        private readonly CsvParser<BigDataSet> _parser;
        private MemoryStream? _ms;
        private MemoryStream? _msRun;

        /// <summary>
        /// Initial Size: 25 lines
        /// Multiply by each of those params
        /// 1000, 10000, 100000, 1000000 lines
        /// </summary>
        [Params(40, 400, 4000, 40000)]
        public int MultiplySize { get; set; }

        public BigDataSetBenchmark()
        {
            _parser = SetUpParser();
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            using var fs = File.OpenRead("big-tiny.csv");
            _ms = new MemoryStream();
            for (int i = 0; i < MultiplySize; i++)
            {
                if (i == 0)
                {
                    fs.CopyTo(_ms);
                }
                else
                {
                    // Skip headers
                    fs.Seek(85, SeekOrigin.Begin);
                    fs.CopyTo(_ms);
                }
            }
        }

        #region Setup

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            _ms!.Dispose();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            _msRun = new MemoryStream(_ms!.ToArray());
            _msRun.Seek(0, SeekOrigin.Begin);
        }

        [IterationCleanup]
        public void IterationCleanup()
        {
            _msRun!.Dispose();
        }

        #endregion

        [Benchmark]
        public async Task<IReadOnlyList<BigDataSet>> Benchmark()
        {
            return await _parser.ParseStream(_msRun!, new CsvConfiguration(','));
        }

        private static CsvParser<BigDataSet> SetUpParser()
        {
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
