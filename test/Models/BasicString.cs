namespace FluentCsvMachine.Test.Models
{
    internal class BasicString
    {
        [CsvNoHeader(columnIndex: 0)] public string? A { get; set; }

        [CsvNoHeader(columnIndex: 1)] public string? B { get; set; }

        public string? C { get; set; }

        [CsvNoHeader(columnIndex: 2)] public string? D { get; set; }
    }
}