using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCsvMachine.Machine.Result
{
    readonly struct ResultLine
    {
        public ResultLine()
        {
        }


        public int LineNumber { get; }

        public ResultValue[] Fields { get; }
    }
}
