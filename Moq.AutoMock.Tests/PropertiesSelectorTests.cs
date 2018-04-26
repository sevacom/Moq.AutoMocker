﻿using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Moq.AutoMock.Tests
{
    [TestFixture]
    public class PropertiesSelectorTests
    {
        [Test]
        public void ShouldGetPublicProperties_WithAttribute_WithReferenceType()
        {
            var properties = GetProperties<ServiceWithProperties>(
                BindingFlags.Instance | BindingFlags.Public,
                p => p.WithAttribute<ImportPropertyAttribute>());

            Assert.AreEqual(
                ServiceWithProperties.PublicPropertiesWithImportAttributeAndRefereceTypeCount,
                properties.Count());
        }

        [Test]
        public void ShouldGetPublicProperties_WithReferenceType()
        {
            var properties = GetProperties<ServiceWithProperties>(
                BindingFlags.Instance | BindingFlags.Public);

            Assert.AreEqual(ServiceWithProperties.PublicRefereceTypePropertiesCount,
                properties.Count());
        }

        [Test]
        public void ShouldGetPublicProperties_WithSettersOnly()
        {
            var properties = GetProperties<ServiceWithProperties>(
                BindingFlags.Instance | BindingFlags.Public,
                p => p.WithSetters());

            Assert.AreEqual(ServiceWithProperties.PublicRefereceTypePropertiesWithSetterCount,
                properties.Count());
        }

        private IEnumerable<PropertyInfo> GetProperties<T>(BindingFlags bindingFlags, Action<PropertiesSelector<T>> setupValidator = null)
            where T : class
        {
            var validator = PropertiesSelector<T>.Create();
            setupValidator?.Invoke(validator);
            return PropertiesSelector<T>.GetProperties(bindingFlags, p => validator.IsPropertyValid(p));
        }
    }

    public class ServiceWithProperties
    {
        public const int AllPropertiesCount = 9;
        public const int PublicPropertiesWithImportAttributeAndRefereceTypeCount = 4;
        public const int PublicRefereceTypePropertiesCount = 6;
        public const int PublicRefereceTypePropertiesWithSetterCount = 5;

        [ImportProperty]
        public IService1 GetSetProperty { get; set; }

        [ImportProperty]
        public IService5 GetProperty { get; }

        [ImportProperty]
        public IService2 GetSetPropertyWithValue { get; set; }

        [ImportProperty]
        public Service2 GetSetClassProperty { get; set; }

        private IService2 GetSetPrivateProperty { get; set; }

        public IService3 GetSetPropertyWithoutAttribute1 { get; set; }

        public IService3 GetSetPropertyWithoutAttribute2 { get; set; }

        public int ValueTypeProperty { get; set; }

        [ImportProperty]
        public int ValueTypeImportProperty { get; set; }

        public ServiceWithProperties()
        {
            GetSetPropertyWithValue = new Service2();
        }
    }

    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
    public class ImportPropertyAttribute : Attribute
    {

    }
}
