using FluentCsvMachine.Helpers;

namespace FluentCsvMachine.Property
{
    /// <summary>
    /// Map a column by an action
    /// </summary>
    /// <typeparam name="T">Type of the entity</typeparam>
    /// <typeparam name="V">Type of the column (string, int, ...)</typeparam>
    public class CsvPropertyCustom<T, V> : CsvPropertyBase
    {
        private readonly Action<T, V> _customAction;

        /// <summary>
        /// Custom action based on a column property
        /// </summary>
        public CsvPropertyCustom(Type propertyType, Action<T, V> customAction) : base(propertyType, true)
        {
            Guard.IsNotNull(customAction);
            _customAction = customAction;
        }

        /// <summary> 
        /// Defines a custom mapping based on a CSV column
        /// </summary>
        /// <param name="entity">Entity for value assignment</param>
        /// <param name="parsedValue">Parsed CSV field</param>
        public void CustomAction(T entity, V parsedValue)
        {
            _customAction(entity, parsedValue);
        }
    }
}