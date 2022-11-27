using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Result;
using FluentCsvMachine.Machine.Workflow;

namespace FluentCsvMachine.Machine
{
    /// <summary>
    /// bool ProcessLine(ResultLine?[] lines, out IReadOnlyList&lt;T&gt; entities)
    /// </summary>
    /// <typeparam name="T">Type of the entity (csv line)</typeparam>
    /// <param name="lines">Dequeued CSV lines</param>
    /// <param name="entities">Converted CSV lines</param>
    /// <returns>The last value of lines was null ? break -> CSV file fully read : false</returns>
    internal delegate bool ProcessResultDelegate<T>(ResultLine?[] lines, out IReadOnlyList<T> entities);

    /// <summary>
    /// Workflow for CSV parsing and entity creation
    /// </summary>
    /// <typeparam name="T">Type of the entity (csv line)</typeparam>
    internal class Workflow<T> where T : new()
    {
        private readonly WorkflowInput<T> input;
        private readonly CsvMachine<T> csv;
        private readonly Stream stream;
        private readonly FastQueue queue;
        private readonly ProcessResultDelegate<T> processDelegate;

        internal Workflow(WorkflowInput<T> input)
        {
            Guard.IsNotNull(input);

            this.input = input;
            queue = new FastQueue(input.Config.EntityQueueSize);
            csv = new CsvMachine<T>(input, queue.Insert);
            stream = input.Stream;

            processDelegate = ProcessLine;
        }

        /// <summary>
        /// Starts the parsing workflow
        /// Creates threads to do the job
        /// </summary>
        /// <returns>Parsed CSV file</returns>
        internal async Task<IReadOnlyList<T>> Start()
        {
            if (!input.SearchForHeaders)
            {
                // No headers do exist, directly start with the content
                csv.SetStateContent();
            }

            // Start the producer an consumer
            var csvReader = Task.Run(ParseStream);
            var entityCreator = Task.Run(CreateResult);

            await csvReader;
            await entityCreator;

            return entityCreator.Result;
        }

        /// <summary>
        /// Parse CSV stream
        /// </summary>
        /// <remarks>Do not use this method in the main thread!</remarks>
        private void ParseStream()
        {
            using var sr = new StreamReader(stream, csv.Config.Encoding);

            char[] buffer = new char[csv.Config.BufferSize];
            int read;

            while ((read = sr.Read(buffer, 0, buffer.Length)) > 0)
            {
                csv.Process(buffer, read);
            }

            csv.EndOfFile();

            queue.Close();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>Do not use this method in the main thread!</remarks>
        private List<T> CreateResult()
        {
            return queue.Process<T>(processDelegate);
        }


        /// <summary>
        /// Implementation of the delegate <see cref="ProcessResultDelegate{T}"/>
        /// Invoked by queue.Process in order to create entities
        /// </summary>
        /// <param name="lines">Work load parsed CSV lines</param>
        /// <param name="entities">out: created entities</param>
        /// <returns>Null was seen, return true -> complete task</returns>
        private bool ProcessLine(ResultLine?[] lines, out IReadOnlyList<T> entities)
        {
            var result = new List<T>(lines.Length);
            var taskFinished = false;

            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].HasValue)
                {
                    // Queue will not receive further updates
                    taskFinished = true;
                    break;
                }

                var line = lines[i]!.Value;
                var entity = csv.Factory!.Create(ref line);
                result.Add(entity);
            }

            entities = result;
            return taskFinished;
        }
    }
}