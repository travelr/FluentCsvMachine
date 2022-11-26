﻿using FluentCsvMachine.Exceptions;
using FluentCsvMachine.Helpers;
using FluentCsvMachine.Machine.Result;
using FluentCsvMachine.Property;
using System.Linq.Expressions;
using System.Reflection;

namespace FluentCsvMachine.Machine
{
    internal class EntityFactory<T> where T : new()
    {
        private readonly IReadOnlyList<CsvProperty<T>> _properties;
        private readonly IReadOnlyList<CsvPropertyBase> _custom;


        private readonly Action<T, object?>?[] setterCache;
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

            setterCache = new Action<T, object?>?[maxColNumber];
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
                var index = p.Index!.Value;

                // Check if CSV field has a value
                if (!GetValue(line, index, out ResultValue value))
                {
                    continue;
                }

                // Try to get a cached accessor
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


                // Assign to value to the object
                var setter = GetSetter(accessor, index, value.Type!);
                setter(resultObj, ToTypedValue(value));
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
                if (!GetValue(line, custom.Index!.Value, out ResultValue value))
                {
                    continue;
                }

                // ToDo: Cache the method
                var customAction = custom.GetType().GetMethod("CustomAction");
                if (customAction == null)
                {
                    throw new CsvMachineException("EntityFactory algorithm failed, CustomAction method has been renamed!");
                }

                customAction.Invoke(custom, new[] { resultObj, ToTypedValue(value) });
            }
        }

        /// <summary>
        /// Get a value form the CSV line
        /// </summary>
        /// <param name="line">CSV line</param>
        /// <param name="index">Index of the field</param>
        /// <param name="value">ResultValue output</param>
        /// <returns>True if the value is not null</returns>
        private static bool GetValue(IReadOnlyList<ResultValue> line, int index, out ResultValue value)
        {
            // Raw value form CSV
            value = line[index];
            return !value.IsNull;
        }


        private static object? ToTypedValue(ResultValue value)
        {
            Type t = value.Type!;

            // ToDo: Cache it
            // Convert.ChangeType on Nullable does not work, get the GetUnderlyingType
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                t = Nullable.GetUnderlyingType(t)!;
            }

            var result = Convert.ChangeType(value.Value, t);

            return result;
        }

        /// <summary>
        /// Creates or gets a setter to a setter
        /// </summary>
        /// <param name="accessor">Accessor expression to the property</param>
        /// <param name="index">Column index for fast caching</param>
        /// <param name="columnPropertyType">Type defined in the column</param>
        /// <returns>The setter</returns>
        /// <exception cref="Exception">Should not happen, broken code?</exception>
        private Action<T, object?> GetSetter(Expression<Func<T, object>> accessor, int index, Type columnPropertyType)
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
                throw new CsvMachineException("EntityFactory algorithm failed, unknown expression type!");
            }

            if (property.PropertyType != columnPropertyType)
            {
                ThrowHelper.CsvColumnMismatchException(
                    $"The column {property.Name} has the type ({columnPropertyType}) which does not match the class definition ({property.PropertyType})");
            }

            // Based on https://stackoverflow.com/a/17669142
            var exInstance = Expression.Parameter(property.DeclaringType, "t");
            var exMemberAccess = Expression.MakeMemberAccess(exInstance, property);
            var exValue = Expression.Parameter(typeof(object), "p");
            var exConvertedValue = Expression.Convert(exValue, property.PropertyType);
            var exBody = Expression.Assign(exMemberAccess, exConvertedValue);
            var lambda = Expression.Lambda<Action<T, object?>>(exBody, exInstance, exValue);
            var action = lambda.Compile();

            // Add it to the cache
            setterCache[index] = action;

            return action;
        }
    }
}
