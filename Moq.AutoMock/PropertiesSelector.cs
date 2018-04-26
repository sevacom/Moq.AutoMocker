using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Moq.AutoMock
{
    /// <summary>
    /// Help to select and get properties by type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class PropertiesSelector<T> where T: class
    {
        private static Func<PropertyInfo, T, bool> _withSettersFunc = (p, instance) => p.CanWrite;
        private List<Func<PropertyInfo, T, bool>> _checkFunctors = new List<Func<PropertyInfo, T, bool>>();
        private bool _isCheckWithValue = false;

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
               .Where(p => isSetMockProperty(p));

            return properties;
        }

        public PropertiesSelector<T> WithPropertyNames()
        {
            // TODO
            return this;
        }

        public PropertiesSelector<T> WithSetters()
        {
            _checkFunctors.Add(_withSettersFunc);
            return this;
        }

        public PropertiesSelector<T> WithAttribute<TAttribute>()
            where TAttribute: Attribute
        {
            var attributeType = typeof(TAttribute);
            _checkFunctors.Add((p, instance) => p.IsDefined(attributeType, true));
            return this;
        }

        public PropertiesSelector<T> WithValue(object propertyValue)
        {
            _isCheckWithValue = true;
            _checkFunctors.Add((p, instance) => p.GetValue(instance) == propertyValue);
            return this;
        }

        public PropertiesSelector<T> WithCustom(Func<PropertyInfo, object, bool> customFunc)
        {
            _checkFunctors.Add(customFunc);
            return this;
        }

        public bool IsPropertyValid(PropertyInfo prop, T instance = null)
        {
            if (_checkFunctors.Count == 0)
            {
                return true;
            }

            if (instance == null && _isCheckWithValue)
            {
                throw new ArgumentException($"'{nameof(instance)}' should be not null, to check properties by value ('{nameof(WithValue)}')", nameof(instance));
            }

            return _checkFunctors.All(func => func(prop, instance));
        }   
    }
}
