using FluentCsvMachine.Exceptions;
using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Result;
using FluentCsvMachine.Machine.Workflow;
using System.Collections.Concurrent;

namespace FluentCsvMachine.Machine
{
    /// <summary>
    /// bool ProcessLine(ResultLine?[] lines, out IReadOnlyList&lt;T&gt; entities)
    /// </summary>
    /// <typeparam name="T">Type of the entity (csv line)</typeparam>
    /// <param name="lines">Dequeued CSV lines</param>
    /// <param name="entities">Converted CSV lines</param>
    /// <returns>The last value of lines was null ? break -> CSV file fully read : false</returns>
    internal delegate bool ProcessResultDelegate<T>(ResultLine?[] lines, out (int, IReadOnlyList<T>?) entities);

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
        private readonly BlockingCollection<Exception> threadExceptions;


        internal Workflow(WorkflowInput<T> input)
        {
            Guard.IsNotNull(input);

            this.input = input;
            queue = new FastQueue(input.Config.EntityQueueSize);
            csv = new CsvMachine<T>(input, queue.Insert);
            stream = input.Stream;

            processDelegate = ProcessLine;
            threadExceptions = new BlockingCollection<Exception>();
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


            var producer = Task.Run(ParseStream);
            var consumer = Task.Run(CreateResult);

            await producer;
            await consumer;

            if (threadExceptions.Count > 0)
            {
                // Marshall exceptions from the threads, if they have failed
                throw new AggregateException(threadExceptions);
            }

            // Order sorted chunks and return the final result
            var result = consumer.Result!.SelectMany(x => x.Item2).ToList();

            return result;
        }


        /// <summary>
        /// Parse CSV stream
        /// </summary>
        /// <remarks>Do not use this method in the main thread!</remarks>
        private void ParseStream()
        {
            bool close = true;

            try
            {
                using var sr = new StreamReader(stream, csv.Config.Encoding);

                char[] buffer = new char[csv.Config.BufferSize];
                int read;

                while ((read = sr.Read(buffer, 0, buffer.Length)) > 0)
                {
                    csv.Process(buffer, read);
                }

                csv.EndOfFile();
            }
            catch (CsvConsumerThreadDiedException)
            {
                // Do nothing everything has been handled
                close = false;
            }
            catch (Exception e)
            {
                threadExceptions.Add(e);
            }
            finally
            {
                if (close)
                {
                    // Avoid a deadlock always close the queue
                    queue.Close();
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>Do not use this method in the main thread!</remarks>
        private IReadOnlyList<(int, IReadOnlyList<T>)>? CreateResult()
        {
            try
            {
                return queue.Process(processDelegate);
            }
            catch (Exception e)
            {
                threadExceptions.Add(e);
                queue.CancelProducer();
                return default(IReadOnlyList<(int, IReadOnlyList<T>)>);
            }
        }


        /// <summary>
        /// Implementation of the delegate <see cref="ProcessResultDelegate{T}"/>
        /// Invoked by queue.Process in order to create entities
        /// </summary>
        /// <param name="lines">Work load parsed CSV lines</param>
        /// <param name="entities">out: (line number of the first entity, created entities)</param>
        /// <returns>Null was seen, return true -> complete task</returns>
        private bool ProcessLine(ResultLine?[] lines, out (int, IReadOnlyList<T>?) entities)
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


            entities = lines[0].HasValue ? (lines[0]!.Value.LineNumber, result) : (-1, null);
            return taskFinished;
        }
    }
}