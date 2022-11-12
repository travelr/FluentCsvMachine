using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentCsvMachine.Machine
{
    internal class CsvMachine
    {
        internal enum States
        {
            Initial,
            HeaderSearch,
            Content
        }

        private States State;
        private Line Line;

        internal string[] headers = new string[] { "a", "b", "c", "d", "e" };

        public CsvMachine()
        {
            State = States.Initial;
            Line = new Line(this);
        }

        internal void Process(char[] buffer, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (buffer[i] == '\r')
                {
                    continue;
                }

                Line.Process(buffer[i]);
            }
        }

        internal void ResultLine(List<string?> fields)
        {
            Console.WriteLine(fields);
        }

        /// <summary>
        /// Process the final line if the file does not end with a line break
        /// </summary>
        internal void EndOfFile()
        {
            if (Line.State != Line.States.Initial)
            {
                Line.Process('\n');
            }
        }
    }
}