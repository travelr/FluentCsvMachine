using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Result;
using FluentCsvMachine.Machine.Workflow;
using System.Threading.Channels;

namespace FluentCsvMachine.Machine
{
    /// <summary>
    /// Workflow for CSV parsing and entity creation
    /// </summary>
    /// <typeparam name="T">Type of the entity (csv line)</typeparam>
    internal class Workflow<T> where T : new()
    {
        private readonly WorkflowInput<T> _input;
        private readonly Channel<ResultLine> _channel;

        private CsvMachine<T> _csv;
        private readonly Stream _stream;
        private readonly EntityFactory<T> _factory;

        internal Workflow(WorkflowInput<T> input)
        {
            Guard.IsNotNull(input);

            _input = input;
            _channel = Channel.CreateUnbounded<ResultLine>();

            _csv = new CsvMachine<T>(input, _channel.Writer);
            _stream = input.Stream;
        }

        internal IReadOnlyList<T> Start()
        {
            if (!_input.SearchForHeaders)
            {
                // No headers do exist, directly start with the content
                _csv.SetStateContent();
            }

            return ParseStream();
        }


        private List<T> ParseStream()
        {
            using var sr = new StreamReader(_stream, _csv.Config.Encoding);

            char[] buffer = new char[_csv.Config.BufferSize];
            int read;

            while ((read = sr.Read(buffer, 0, buffer.Length)) > 0)
            {
                _csv.Process(buffer, read);
            }

            return _csv.EndOfFile();
        }
    }
}