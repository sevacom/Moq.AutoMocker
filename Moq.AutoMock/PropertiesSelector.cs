using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Moq.AutoMock
{
    internal class PropertiesSelector
    {
        public PropertyInfo[] GetInjectableProperties(
            Type targetType, 
            BindingFlags bindingFlags, 
            bool withSettersOnly, 
            Type propertyAttributeType = null)
        {
            IEnumerable<PropertyInfo> properties = targetType
                .GetProperties(bindingFlags)
                .Where(p => !p.PropertyType.IsValueType);

            if(propertyAttributeType != null)
            {
                properties = properties.Where(p => p.IsDefined(propertyAttributeType, true));
            }

            if(withSettersOnly)
            {
                properties = properties.Where(p => p.CanWrite);
            }

            return properties.ToArray();
        }
    }

    public sealed class PropertiesSelectorBuilder<T> where T : class
    {
        private static Func<PropertyInfo, object, bool> _withSettersFunc = (p, instance) => p.CanWrite;

        private List<Func<PropertyInfo, object, bool>> _checkFunctors = new List<Func<PropertyInfo, object, bool>>();
        private BindingFlags _bindingFlags = BindingFlags.Instance | BindingFlags.Public;
        private bool _withValueCheck = false;

        public static PropertiesSelectorBuilder<T> Create()
        {
            return new PropertiesSelectorBuilder<T>();
        }

        public static PropertiesSelectorBuilder<T> CreateDefault()
        {
            return Create()
                .WithSetters()
                .WithValue(null);
        }

        public static PropertiesSelectorBuilder<T> CreateDefaultWithAttribute<TAttribute>() where TAttribute : Attribute
        {
            return CreateDefault()
                .WithAttribute<TAttribute>();
        }

        private PropertiesSelectorBuilder()
        {

        }

        public PropertiesSelectorBuilder<T> WithPropertyNames()
        {
            // TODO
            return this;
        }

        public PropertiesSelectorBuilder<T> WithSetters()
        {
            _checkFunctors.Add(_withSettersFunc);
            return this;
        }

        public PropertiesSelectorBuilder<T> WithPrivate()
        {
            if(!_bindingFlags.HasFlag(BindingFlags.NonPublic))
            {
                _bindingFlags = _bindingFlags | BindingFlags.NonPublic;
            }
            
            return this;
        }

        public PropertiesSelectorBuilder<T> WithAttribute<TAttribute>() where TAttribute: Attribute
        {
            var attributeType = typeof(TAttribute);
            _checkFunctors.Add(new Func<PropertyInfo, object, bool>(
                (p, instance) => p.IsDefined(attributeType, true)));
            return this;
        }

        public PropertiesSelectorBuilder<T> WithValue(object propertyValue)
        {
            _withValueCheck = true;
            var func = new Func<PropertyInfo, object, bool>(
                (p, instance) => p.GetValue(instance) == propertyValue);
            _checkFunctors.Add(func);
            return this;
        }

        public PropertiesSelectorBuilder<T> WithCustom(Func<PropertyInfo, object, bool> customFunc)
        {
            _checkFunctors.Add(customFunc);
            return this;
        }

        public IEnumerable<PropertyInfo> GetProperties(T instance = null)
        {
            if(instance == null && _withValueCheck)
            {
                throw new ArgumentException($"'{nameof(instance)}' should be not null, to check properties by value ('{nameof(WithValue)}')", nameof(instance));
            }

            var properties = typeof(T).GetProperties(_bindingFlags)
                .Where(p => !p.PropertyType.IsValueType)
                .Where(p => IsPropertyValid(p, instance));

            return properties;
        }

        private bool IsPropertyValid(PropertyInfo prop, object instance)
        {
            if (_checkFunctors.Count == 0)
            {
                return true;
            }

            return _checkFunctors.All(func => func(prop, instance));
        }   
    }
}
