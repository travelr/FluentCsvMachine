﻿using System.Linq.Expressions;

namespace FluentCsvMachine
{
    /// <summary>
    /// CsvProperty correponds to column in a CSV file
    /// </summary>
    /// <typeparam name="T">This property belongs to T type</typeparam>
    /// <typeparam name="V">Value type of the property</typeparam>
    public class CsvProperty<T> : CsvPropertyBase
    {
        public CsvProperty(Type propertyType, Expression<Func<T, object?>> accessor)
        {
            PropertyType = propertyType;
            Accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
        }

        /// <summary>
        /// Accessor of the property
        /// </summary>
        public Expression<Func<T, object?>> Accessor { get; private set; }
    }
}