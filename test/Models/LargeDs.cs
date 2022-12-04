using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// ReSharper disable IdentifierTypo

namespace FluentCsvMachine.Test.Models
{
    public class LargeDs
    {
        public DateTime Time { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Depth { get; set; }
        public double Mag { get; set; }
        public string MagType { get; set; } = null!;
        public int? Nst { get; set; }
        public string Gap { get; set; } = null!;
        public string Dmin { get; set; } = null!;
        public string Rms { get; set; } = null!;
        public string Net { get; set; } = null!;
        public string Id { get; set; } = null!;
        public DateTime Updated { get; set; }
        public string Place { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}
