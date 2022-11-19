using FluentCsvMachine.Exceptions;
using FluentCsvMachine.Machine.Values;
using FluentCsvMachine.Property;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentCsvMachine.Machine
{
    internal class EntityFactory<T> where T : new()
    {
        private readonly IEnumerable<CsvPropertyBase> _properties;

        /// <summary>
        /// Caches accessors expression to a lambda compiled setter
        /// </summary>
        private readonly Action<T, object>?[] setterCache;

        private readonly Expression<Func<T, object>>?[] accessorCache;


        public EntityFactory(IEnumerable<CsvPropertyBase> properties)
        {
            _properties = properties.Where(x => x.Index.HasValue).ToList();

            var maxColNumber = _properties.Max(x => x.Index!.Value) + 1;
            setterCache = new Action<T, object>[maxColNumber];
            accessorCache = new Expression<Func<T, object>>[maxColNumber];
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
                var index = p.Index!.Value;
                var accessor = accessorCache[index];

                if (accessor == null)
                {
                    // get Accessor property
                    var accessorProperty = typeof(CsvProperty<>).MakeGenericType(typeof(T)).GetProperty("Accessor");
                    if (accessorProperty == null)
                    {
                        throw new CsvMachineException(
                            "EntityFactory algorithm failed, Accessor property has been renamed!");
                    }

                    if (accessorProperty.GetValue(p, null) is not Expression<Func<T, object>> ac)
                    {
                        throw new CsvMachineException(
                            "EntityFactory algorithm failed, accessor is null!");
                    }

                    // Add it to the cache
                    accessorCache[index] = ac;

                    accessor = ac;
                }


                // Raw value form CSV
                var resultValue = line[index];
                if (resultValue.IsNull)
                {
                    continue;
                }

                // Convert raw value based on defined methods
                var value = Convert.ChangeType(resultValue.Value, resultValue.Type!);

                // Assign to value to the object
                var setter = GetSetter(accessor, index);
                setter(resultObj, value!);
            }
        }

        /// <summary>
        /// Creates or gets a setter to a setter
        /// </summary>
        /// <param name="accessor">Accessor expression to the property</param>
        /// <param name="index">Column index for fast caching</param>
        /// <returns>The setter</returns>
        /// <exception cref="Exception">Should not happen, broken code?</exception>
        private Action<T, object> GetSetter(Expression<Func<T, object>> accessor, int index)
        {
            var result = setterCache[index];

            // Try to get a cached setter
            if (result != null)
            {
                return result;
            }

            // Try get to get the property
            if (accessor.Body is not MemberExpression selectorExpr)
            {
                var unary = accessor.Body as UnaryExpression;
                selectorExpr = unary?.Operand as MemberExpression ??
                               throw new CsvMachineException("EntityFactory algorithm failed, selectorExpr is null!");
            }

            // Double check if everything could be resolved
            if (selectorExpr is not { Member: PropertyInfo property } || property.DeclaringType == null)
            {
                throw new CsvMachineException(
                    "EntityFactory algorithm failed, unknown expression type!");
            }

            // Based on https://stackoverflow.com/a/17669142
            var exInstance = Expression.Parameter(property.DeclaringType, "t");
            var exMemberAccess = Expression.MakeMemberAccess(exInstance, property);
            var exValue = Expression.Parameter(typeof(object), "p");
            var exConvertedValue = Expression.Convert(exValue, property.PropertyType);
            var exBody = Expression.Assign(exMemberAccess, exConvertedValue);
            var lambda = Expression.Lambda<Action<T, object>>(exBody, exInstance, exValue);
            var action = lambda.Compile();

            // Add it to the cache
            setterCache[index] = action;

            return action;
        }
    }
}
