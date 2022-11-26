using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCsvMachine.Test.Models
{
    public class EmptyColumn
    {
        [CsvNoHeader(0, "yyyy/MM/dd")] public DateTime? A { get; set; }

        [CsvNoHeader(2)] public string? B { get; set; }

        [CsvNoHeader(1)] public byte? C { get; set; }
    }
}
