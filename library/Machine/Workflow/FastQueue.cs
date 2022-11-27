using FluentCsvMachine.Machine.Result;

namespace FluentCsvMachine.Machine.Workflow
{
    /// <summary>
    /// Thread safe queue implementation acting in between reading the CSV file and creating entities
    /// </summary>
    internal class FastQueue
    {
        private readonly ResultLine?[] queue;
        private readonly int max;
        private readonly object sync;

        private int _head;
        private int _tail;
        private int _count;

        /// <summary>
        /// FastQueue
        /// </summary>
        /// <param name="size">CsvConfiguration.EntityQueueSize</param>
        public FastQueue(int size)
        {
            queue = new ResultLine?[size];
            max = size;
            sync = new object();

            _head = 0;
            _tail = -1;
            _count = 0;
        }

        /// <summary>
        /// Inserts a CSV line into the queue
        /// </summary>
        /// <param name="item">CSV line</param>
        /// <remarks>May block this thread if the queue is full</remarks>
        public void Insert(ResultLine item)
        {
            InsertInternal(item);
        }

        private void InsertInternal(ResultLine? item)
        {
            lock (sync)
            {
                while (true)
                {
                    if (_count == max)
                    {
                        // Queue is full
                        Console.WriteLine("Full");
                        Monitor.Wait(sync);
                    }
                    else
                    {
                        _tail = (_tail + 1) % max;
                        queue[_tail] = item;
                        _count++;
                        Monitor.Pulse(sync);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// No further lines: <para />
        /// Adds null to the queue which means the CSV file has been read completely 
        /// </summary>
        /// <remarks>May block this thread until the lock is received</remarks>
        public void Close()
        {
            InsertInternal(null);
        }

        /// <summary>
        /// Method to dequeue the queue and convert those CSV line values to entities of type T
        /// </summary>
        /// <typeparam name="T">Entity type T</typeparam>
        /// <param name="delegate"> 
        /// ProcessResultDelegate -> bool ProcessLine(ResultLine?[] lines, out IReadOnlyList&lt;T&gt; entities) <para />
        /// out variable: Converted CSV lines to entities <para />
        /// return value: true -> null detected -> break while -> complete Task <para />
        /// </param>
        /// <returns>Created entities by this thread</returns>
        /// <remarks>May block this thread until all CSV lines have been converted to entities of type T</remarks>
        public List<T> Process<T>(ProcessResultDelegate<T> @delegate)
        {
            var result = new List<T>(500);

            // Iterate while until you find a null value
            while (true)
            {
                ResultLine?[]? work = null;

                lock (sync)
                {
                    if (_count == 0)
                    {
                        //Queue is Empty
                        Monitor.Wait(sync);
                    }
                    else
                    {
                        if (_head + _count <= max)
                        {
                            work = queue.Skip(_head).Take(_count).ToArray();
                            _head = (_head + _count) % max; // modulo due to if equals max
                        }
                        else
                        {
                            // _head + _count > max
                            // 1 + 5 > 5
                            // -> 4 = 5 - 1
                            // -> 1 = 5 - 4 -> _head

                            var take1 = max - _head;
                            var take2 = _count - take1;

                            work = queue.Skip(_head).Take(take1).Concat(queue.Take(take2)).ToArray();
                            _head = take2;
                        }

                        _count = 0;
                        Monitor.Pulse(sync);
                    }
                }

                if (work == null)
                {
                    // Queue was empty, go again
                    continue;
                }

                // Create entities by calling the delegate
                var breakWhile = @delegate.Invoke(work!, out IReadOnlyList<T> entities);

                if (entities.Count > 0)
                {
                    // Save entities to result, but not the finish marker
                    result.AddRange(entities);
                }

                if (breakWhile)
                {
                    // Null as a finish marker was seen, CSV file is done, complete this task
                    break;
                }
            }

            return result;
        }
    }
}
