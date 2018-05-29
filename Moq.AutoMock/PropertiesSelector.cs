using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Moq.AutoMock
{
    /// <summary>
    /// Help to select and get properties by type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class PropertiesSelector<T>
        where T : class
    {
        private static readonly Func<PropertyInfo, T, bool> WithSettersFunc = (p, instance) => p.CanWrite;
        private readonly List<Func<PropertyInfo, T, bool>> _checkFunctors = new List<Func<PropertyInfo, T, bool>>();
        private bool _isAnyChecksWithValue;
        private List<string> _propertyNames;

        private PropertiesSelector()
        {
        }

        public static PropertiesSelector<T> Create()
        {
            return new PropertiesSelector<T>();
        }

        public static IEnumerable<PropertyInfo> GetProperties(
            BindingFlags bindingFlags,
            Func<PropertyInfo, bool> isSetMockProperty)
        {
            var properties = typeof(T).GetProperties(bindingFlags)
               .Where(p => !p.PropertyType.IsValueType)
               .Where(isSetMockProperty);

            return properties;
        }

        public PropertiesSelector<T> WithPropertyNames(params string[] propertyNames)
        {
            if (_propertyNames == null)
            {
                _propertyNames = new List<string>();
                _checkFunctors.Add((p, instance) => _propertyNames.Contains(p.Name));
            }

            _propertyNames.AddRange(propertyNames);

            return this;
        }

        public PropertiesSelector<T> WithPropertyName<TProperty>(
            Expression<Func<T, TProperty>> propertyExpression)
        {
            return WithPropertyNames(GetPropertyName(propertyExpression));
        }

        public PropertiesSelector<T> WithSetters()
        {
            _checkFunctors.Add(WithSettersFunc);
            return this;
        }

        public PropertiesSelector<T> WithAttribute<TAttribute>()
            where TAttribute : Attribute
        {
            var attributeType = typeof(TAttribute);
            _checkFunctors.Add((p, instance) => p.IsDefined(attributeType, true));
            return this;
        }

        public PropertiesSelector<T> WithValue(object propertyValue)
        {
            _isAnyChecksWithValue = true;
            _checkFunctors.Add((p, instance) => p.GetValue(instance) == propertyValue);
            return this;
        }

        public PropertiesSelector<T> WithCustom(Func<PropertyInfo, T, bool> customFunc)
        {
            _checkFunctors.Add(customFunc);
            return this;
        }

        public bool IsPropertyApplicable(PropertyInfo prop, T instance = null)
        {
            if (_checkFunctors.Count == 0)
            {
                return true;
            }

            if (instance == null && _isAnyChecksWithValue)
            {
                throw new ArgumentException($"'{nameof(instance)}' should be not null, to check properties by value ('{nameof(WithValue)}')", nameof(instance));
            }

            return _checkFunctors.All(func => func(prop, instance));
        }

        private static string GetPropertyName<TSource, TProperty>(
            Expression<Func<TSource, TProperty>> propertyLambda)
        {
            var type = typeof(TSource);

            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a method, not a property.");
            }

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a field, not a property.");
            }

            if (type != propInfo.ReflectedType && !type.IsSubclassOf(propInfo.ReflectedType))
            {
                throw new ArgumentException($"Expression '{propertyLambda}' refers to a property that is not from type {type}.");
            }

            return propInfo.Name;
        }
    }
}