using NUnit.Framework;
using System;
using System.Linq;
using System.Reflection;

namespace Moq.AutoMock.Tests
{
    [TestFixture]
    public class PropertiesSelectorTests
    {
        private readonly PropertiesSelector _target = new PropertiesSelector();

        [Test]
        public void ShouldGetPublicProperties_WithAttribute_WithReferenceType()
        {
            var properties = _target.GetInjectableProperties(
                typeof(ServiceWithProperties),
                 BindingFlags.Instance | BindingFlags.Public,
                 false,
                 typeof(ImportPropertyAttribute));

            properties = PropertiesSelectorBuilder<ServiceWithProperties>
                .Create()
                .WithAttribute<ImportPropertyAttribute>()
                .GetProperties()
                .ToArray();

            Assert.AreEqual(
                ServiceWithProperties.PublicPropertiesWithImportAttributeAndRefereceTypeCount,
                properties.Count());
        }

        [Test]
        public void ShouldGetPublicProperties_WithReferenceType()
        {
            var properties = _target.GetInjectableProperties(
                typeof(ServiceWithProperties),
                 BindingFlags.Instance | BindingFlags.Public,
                 false);

            properties = PropertiesSelectorBuilder<ServiceWithProperties>
                .Create()
                .GetProperties()
                .ToArray();

            Assert.AreEqual(ServiceWithProperties.PublicRefereceTypePropertiesCount,
                properties.Count());
        }

        [Test]
        public void ShouldGetPublicProperties_WithSettersOnly()
        {
            var properties = _target.GetInjectableProperties(
                typeof(ServiceWithProperties),
                 BindingFlags.Instance | BindingFlags.Public,
                 true);

            properties = PropertiesSelectorBuilder<ServiceWithProperties>
                .Create()
                .WithSetters()
                .GetProperties()
                .ToArray();

            Assert.AreEqual(ServiceWithProperties.PublicRefereceTypePropertiesWithSetterCount,
                properties.Count());
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
