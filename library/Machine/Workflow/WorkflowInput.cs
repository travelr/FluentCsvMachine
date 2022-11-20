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
        internal WorkflowInput(Stream stream, List<CsvPropertyBase> properties)
        {
            Guard.IsNotNull(stream);
            Guard.IsNotNull(properties);

            Stream = stream;
            Properties = properties;
        }

        internal Stream Stream { get; }

        internal List<CsvPropertyBase> Properties { get; }


        internal CsvConfiguration? Config { get; set; }

        internal List<Action<T, IReadOnlyList<object?>>>? LineActions { get; set; }
    }
}
