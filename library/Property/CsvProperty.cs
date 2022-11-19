using FluentCsvMachine.Helpers;
using System.Linq.Expressions;

namespace FluentCsvMachine.Property
{
    /// <summary>
    /// CsvProperty correspond to column in a CSV file
    /// </summary>
    /// <typeparam name="T">This property belongs to T type</typeparam>
    public class CsvProperty<T> : CsvPropertyBase
    {
        public CsvProperty(Type propertyType, Expression<Func<T, object?>> accessor) : base(propertyType, false)
        {
            Guard.IsNotNull(accessor);
            Accessor = accessor;
        }


        /// <summary>
        /// Accessor of the property
        /// </summary>
        public Expression<Func<T, object?>> Accessor { get; }
    }
}