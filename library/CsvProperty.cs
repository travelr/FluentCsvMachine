using System.Linq.Expressions;

namespace readerFlu
{
    /// <summary>
    /// CsvProperty correponds to column in a CSV file
    /// </summary>
    /// <typeparam name="T">This property belongs to T type</typeparam>
    /// <typeparam name="V">Value type of the property</typeparam>
    public class CsvProperty<T, V> : CsvPropertyBase
    {
        public CsvProperty(Expression<Func<T, V>> accessor)
        {
            Accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        /// <summary>
        /// Accessor of the property
        /// </summary>
        public Expression<Func<T, V>> Accessor { get; private set; }
    }
}