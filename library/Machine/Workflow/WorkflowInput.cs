using FluentCsvMachine.Helpers;
using FluentCsvMachine.Property;

namespace FluentCsvMachine.Machine.Workflow
{
    internal class WorkflowInput<T>
    {
        /// <summary>
        /// Object in order to create a workflow
        /// Assign nullable objects outside of the constructor
        /// </summary>
        /// <param name="stream">Stream of the CSV file</param>
        /// <param name="properties">List of defined properties</param>
        /// <param name="searchForHeaders">True: Header needs to be found in CSV, False: Columns are predefined via CsvNoHeaderAttribute</param>
        /// <param name="config"><see cref="CsvConfiguration"/> - if null we are using the defaults</param>
        internal WorkflowInput(Stream stream, List<CsvPropertyBase> properties, bool searchForHeaders, CsvConfiguration? config)
        {
            Guard.IsNotNull(stream);
            Guard.IsNotNull(properties);

            Stream = stream;
            Properties = properties;
            SearchForHeaders = searchForHeaders;
            Config = config ?? new CsvConfiguration();


            if (Config.EntityQueueSize < 20 || Config.EntityQueueSize <= Config.FactoryThreads)
            {
                ThrowHelper.ThrowCsvConfigurationException("Please choose a larger queue size. Values larger 20 are valid");
            }
            else if (Config.FactoryThreads > 10)
            {
                ThrowHelper.ThrowCsvConfigurationException(
                    "Consider please a lower number of threads. Too many context changes will slow you down. Max. value is 10.");
            }
        }


        internal Stream Stream { get; }

        internal List<CsvPropertyBase> Properties { get; }

        public bool SearchForHeaders { get; }


        internal CsvConfiguration Config { get; }

        internal List<Action<T, IReadOnlyList<object?>>>? LineActions { get; set; }
    }
}
