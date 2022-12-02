using FluentCsvMachine.Helpers;
using System.Linq.Expressions;

namespace FluentCsvMachine.Property
{
    /// <summary>
    /// CsvProperty corresponds to column in a CSV file
    /// </summary>
    /// <typeparam name="T">This property belongs to T type</typeparam>
    public class CsvProperty<T> : CsvPropertyBase
    {
        /// <summary>
        /// CsvProperty corresponds to column in a CSV file
        /// </summary>
        /// <param name="propertyType">Value type of the property</param>
        /// <param name="accessor">Expression to the property which shall be mapped</param>
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