namespace FluentCsvMachine.Property
{
    public class CsvPropertyCustom : CsvPropertyBase
    {
        /// <summary>
        /// Custom action based on a column property
        /// </summary>
        public CsvPropertyCustom(Action<object, string> customAction)
        {
            CustomAction = customAction;
        }

        /// <summary>
        /// Defines a custom mapping based on a CSV column
        /// Action(Entity for value assignment, csv value)
        /// </summary>
        public Action<object, string> CustomAction { get; }
    }
}