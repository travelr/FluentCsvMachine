using FluentCsvMachine.Exceptions;
using FluentCsvMachine.Property;
using System.Linq.Expressions;
using System.Reflection;
using FluentCsvMachine.Machine.Result;

namespace FluentCsvMachine.Machine
{
    internal class EntityFactory<T> where T : new()
    {
        private readonly IReadOnlyList<CsvProperty<T>> _properties;
        private readonly IReadOnlyList<CsvPropertyBase> _custom;


        private readonly Action<T, object>?[] setterCache;
        private readonly Expression<Func<T, object>>?[] accessorCache;

        private readonly List<Action<T, IReadOnlyList<object?>>>? _lineActions;


        public EntityFactory(IEnumerable<CsvPropertyBase> properties, List<Action<T, IReadOnlyList<object?>>>? lineActions)
        {
            var validProperties = properties.Where(x => x.Index.HasValue).ToList();

            var maxColNumber = validProperties.Max(x => x.Index!.Value) + 1;
            if (maxColNumber < 1)
            {
                throw new CsvMachineException("EntityFactory algorithm failed, set Index on properties first");
            }

            setterCache = new Action<T, object>[maxColNumber];
            accessorCache = new Expression<Func<T, object>>[maxColNumber];

            _properties = validProperties.Where(x => !x.IsCustom).Cast<CsvProperty<T>>().ToList();
            _custom = validProperties.Where(x => x.IsCustom).ToList();


            _lineActions = lineActions;
        }


        internal T Create(IReadOnlyList<ResultValue> line)
        {
            var resultObj = new T();

            // Set the properties of the object based on the CSV line
            SetProperties(line, resultObj);

            // Execute custom columns after the normal ones 
            if (_custom.Count > 0)
            {
                CustomColumns(line, resultObj);
            }

            // Execute line actions always last
            if (_lineActions is { Count: > 0 })
            {
                var arg = line.Select(x => x.Value).ToList();
                foreach (var action in _lineActions)
                {
                    action(resultObj, arg);
                }
            }

            return resultObj;
        }

        /// <summary>
        /// Sets the properties of the object based on the CSV line
        /// </summary>
        /// <param name="line">CSV line</param>
        /// <param name="resultObj">corresponding object</param>
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


                if (!GetValue(line, index, out object? value))
                {
                    continue;
                }

                // Assign to value to the object
                var setter = GetSetter(accessor, index);
                setter(resultObj, value!);
            }
        }

        /// <summary>
        /// Runs customMappingsColumn or customMappingsLine
        /// </summary>
        /// <param name="line">Current CSV line</param>
        /// <param name="resultObj">Entity for value assignment</param>
        private void CustomColumns(IReadOnlyList<ResultValue> line, T resultObj)
        {
            foreach (var custom in _custom)
            {
                if (!GetValue(line, custom.Index!.Value, out object? value))
                {
                    continue;
                }

                // ToDo: Cache the method
                var customAction = custom.GetType().GetMethod("CustomAction");
                if (customAction == null)
                {
                    throw new CsvMachineException("EntityFactory algorithm failed, CustomAction method has been renamed!");
                }

                customAction.Invoke(custom, new[] { resultObj, value });
            }
        }

        /// <summary>
        /// From CSV line to typed field value
        /// </summary>
        /// <param name="line">CSV line</param>
        /// <param name="index">Index of the field</param>
        /// <param name="value">typed value, may be null if the CSV field was empty or was not valid</param>
        /// <returns></returns>
        private static bool GetValue(IReadOnlyList<ResultValue> line, int index, out object? value)
        {
            // Raw value form CSV
            var resultValue = line[index];
            if (resultValue.IsNull)
            {
                value = null;
                return false;
            }

            // Convert raw value based on defined methods
            value = Convert.ChangeType(resultValue.Value, resultValue.Type!);
            return true;
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
