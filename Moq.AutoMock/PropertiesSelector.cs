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
}
