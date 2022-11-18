using FluentCsvMachine.Machine.Values;
using FluentCsvMachine.Property;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentCsvMachine.Machine
{
    internal class EntityFactory<T> where T : new()
    {
        /// <summary>
        /// Caches accessors expression to a lambda compiled setter
        /// </summary>
        private readonly Dictionary<Expression<Func<T, object>>, Action<T, object>> setterCache = new();

        private readonly Dictionary<string, Expression<Func<T, object>>> accessorCache = new();

        private readonly IEnumerable<CsvPropertyBase> _properties;

        public EntityFactory(IEnumerable<CsvPropertyBase> properties)
        {
            _properties = properties;
        }

        internal T Create(IReadOnlyList<ResultValue> line)
        {
            var resultObj = new T();

            // Set the properties of the object based on the CSV line
            SetProperties(line, resultObj);

            return resultObj;
        }

        /// <summary>
        /// Sets the properties of the object based on the CSV line
        /// </summary>
        /// <param name="line">CSV line</param>
        /// <param name="resultObj">corresponding object</param>
        /// <exception cref="Exception">CsvProperty has no property Accessor</exception>
        private void SetProperties(IReadOnlyList<ResultValue> line, T resultObj)
        {
            foreach (var p in _properties)
            {
                // Try to get a cached accessor
                if (!accessorCache.TryGetValue(p.ColumnName!, out var accessor))
                {
                    // get Accessor property
                    var accessorProperty = typeof(CsvProperty<>).MakeGenericType(typeof(T)).GetProperty("Accessor");
                    if (accessorProperty == null)
                    {
                        throw new Exception("Accessor property has been renamed!");
                    }

                    if (accessorProperty.GetValue(p, null) is not Expression<Func<T, object>> ac)
                    {
                        throw new Exception("accessor is null");
                    }

                    accessor = ac;
                }


                // Raw value form CSV
                var i = p.Index!.Value;
                var resultValue = line[i];
                if (resultValue.IsNull)
                {
                    continue;
                }

                // Convert raw value based on defined methods
                var value = Convert.ChangeType(resultValue.Value, resultValue.Type!);

                // Assign to value to the object
                var setter = GetSetter(accessor);
                setter(resultObj, value!);
            }
        }

        /// <summary>
        /// Creates or gets a setter to a setter
        /// </summary>
        /// <param name="accessor">Accessor expression to the property</param>
        /// <returns>The setter</returns>
        /// <exception cref="Exception">Should not happen, broken code?</exception>
        private Action<T, object> GetSetter(Expression<Func<T, object>> accessor)
        {
            // Try to get a cached setter
            if (setterCache.TryGetValue(accessor, out var result))
            {
                return result;
            }

            // Try get to get the property
            MemberExpression? selectorExpr = accessor.Body as MemberExpression;
            if (selectorExpr == null)
            {
                var unary = accessor.Body as UnaryExpression;
                selectorExpr = unary?.Operand as MemberExpression;
            }

            // Double check if everything could be resolved
            if (selectorExpr is not { Member: PropertyInfo property } || property.DeclaringType == null)
            {
                throw new Exception("unknown expression type");
            }

            // Based on https://stackoverflow.com/a/17669142
            var exInstance = Expression.Parameter(property.DeclaringType, "t");
            var exMemberAccess = Expression.MakeMemberAccess(exInstance, property);
            var exValue = Expression.Parameter(typeof(object), "p");
            var exConvertedValue = Expression.Convert(exValue, property.PropertyType);
            var exBody = Expression.Assign(exMemberAccess, exConvertedValue);
            var lambda = Expression.Lambda<Action<T, object>>(exBody, exInstance, exValue);
            var action = lambda.Compile();

            if (!setterCache.TryAdd(accessor, action))
            {
                Console.WriteLine();
            }

            return action;
        }
    }
}
