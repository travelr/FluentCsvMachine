using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCsvMachine.Exceptions
{
    /// <summary>
    /// Internal exception: State machines cannot handle CSV file
    /// </summary>
    public class CsvMachineException : Exception
    {
        public CsvMachineException() : base("State machines cannot handle CSV file, please support a bug ticket => Unkown state transition")
        {
        }
    }
}