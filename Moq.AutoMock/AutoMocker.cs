using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Moq.Language.Flow;

namespace Moq.AutoMock
{
    /// <summary>
    /// An auto-mocking IoC container that generates mock objects using Moq.
    /// </summary>
    public partial class AutoMocker
    {
        private readonly Dictionary<Type, IInstance> _typeMap = new Dictionary<Type, IInstance>();
        private readonly ConstructorSelector _constructorSelector = new ConstructorSelector();
        private readonly MockBehavior _mockBehavior;

        public AutoMocker(MockBehavior mockBehavior)
        {
            this._mockBehavior = mockBehavior;
        }

        public AutoMocker() : this(MockBehavior.Default)
        {
        }

        /// <summary>
        /// Constructs an instance from known services. Any dependancies (constructor arguments)
        /// are fulfilled by searching the container or, if not found, automatically generating
        /// mocks.
        /// </summary>
        /// <typeparam name="T">A concrete type</typeparam>
        /// <returns>An instance of T with all constructor arguments derived from services
        /// setup in the container.</returns>
        public T CreateInstance<T>()
            where T : class
        {
            return CreateInstance<T>(false);
        }

        /// <summary>
        /// Constructs an instance from known services. Any dependancies (constructor arguments)
        /// are fulfilled by searching the container or, if not found, automatically generating
        /// mocks.
        /// </summary>
        /// <typeparam name="T">A concrete type</typeparam>
        /// <param name="enablePrivate">When true, private constructors will also be used to
        /// create mocks.</param>
        /// <returns>An instance of T with all constructor arguments derrived from services
        /// setup in the container.</returns>
        public T CreateInstance<T>(bool enablePrivate)
            where T : class
        {
            var bindingFlags = GetBindingFlags(enablePrivate);
            var arguments = CreateArguments<T>(bindingFlags);
            try
            {
                return (T)Activator.CreateInstance(typeof(T), bindingFlags, null, arguments, null);
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException.PreserveStackTrace();
            }
        }

        /// <summary>
        /// Set instance properties with mocks by default: with setters and null values
        /// </summary>
        public void MockProperties<T>(T instance)
            where T : class
        {
            MockProperties(
                instance,
                p => p.WithSetters().WithValue(null));
        }

        /// <summary>
        /// Set instance properties with mocks (used TPropertyAttribute type to find properties that set)
        /// </summary>
        public void MockProperties<T, TPropertyAttribute>(T instance)
            where T : class
            where TPropertyAttribute : Attribute
        {
            MockProperties(
                instance,
                p => p
                    .WithSetters()
                    .WithValue(null)
                    .WithAttribute<TPropertyAttribute>());
        }

        /// <summary>
        /// Set instance properties with mocks
        /// </summary>
        public void MockProperties<T>(
            T instance,
            Action<PropertiesSelector<T>> setupSelector,
            bool enablePrivate = false)
            where T : class
        {
            var propertiesSelector = PropertiesSelector<T>.Create();
            setupSelector(propertiesSelector);
            MockProperties(instance, p => propertiesSelector.IsPropertyApplicable(p, instance), enablePrivate);
        }

        /// <summary>
        /// Set instance properties with mocks
        /// </summary>
        public void MockProperties<T>(
            T instance,
            Func<PropertyInfo, bool> customMockPropertySelector,
            bool enablePrivate)
            where T : class
        {
            var properties = PropertiesSelector<T>.GetProperties(
                GetBindingFlags(enablePrivate),
                customMockPropertySelector);

            MockProperties(instance, properties);
        }

        private void MockProperties<T>(T instance, IEnumerable<PropertyInfo> properties)
            where T : class
        {
            foreach (var injectProperty in properties)
            {
                var mockedPropertyValue = GetObjectFor(injectProperty.PropertyType);
                injectProperty.SetValue(instance, mockedPropertyValue);
            }
        }

        private static BindingFlags GetBindingFlags(bool enablePrivate)
        {
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            if (enablePrivate)
            {
                bindingFlags = bindingFlags | BindingFlags.NonPublic;
            }

            return bindingFlags;
        }

        private object[] CreateArguments<T>(BindingFlags bindingFlags)
            where T : class
        {
            var ctor = _constructorSelector.SelectFor(typeof(T), _typeMap.Keys.ToArray(), bindingFlags);
            var arguments = ctor.GetParameters().Select(x => GetObjectFor(x.ParameterType)).ToArray();
            return arguments;
        }

        /// <summary>
        /// Constructs a self-mock from the services available in the container. A self-mock is
        /// a concrete object that has virtual and abstract members mocked. The purpose is so that
        /// you can test the majority of a class but mock out a resource. This is great for testing
        /// abstract classes, or avoiding breaking cohesion even further with a non-abstract class.
        /// </summary>
        /// <typeparam name="T">The instance that you want to build</typeparam>
        /// <returns>An instance with virtual and abstract members mocked</returns>
        public T CreateMockObject<T>()
            where T : class
        {
            return CreateMock<T>(false).Object;
        }

        /// <summary>
        /// Constructs a self-mock from the services available in the container. A self-mock is
        /// a concrete object that has virtual and abstract members mocked. The purpose is so that
        /// you can test the majority of a class but mock out a resource. This is great for testing
        /// abstract classes, or avoiding breaking cohesion even further with a non-abstract class.
        /// </summary>
        /// <typeparam name="T">The instance that you want to build</typeparam>
        /// <param name="enablePrivate">When true, private constructors will also be used to
        /// create mocks.</param>
        /// <returns>An instance with virtual and abstract members mocked</returns>
        public T CreateMockObject<T>(bool enablePrivate)
            where T : class
        {
            return CreateMock<T>(false).Object;
        }

        /// <summary>
        /// Constructs a self-mock from the services available in the container. A self-mock is
        /// a concrete object that has virtual and abstract members mocked. The purpose is so that
        /// you can test the majority of a class but mock out a resource. This is great for testing
        /// abstract classes, or avoiding breaking cohesion even further with a non-abstract class.
        /// </summary>
        /// <typeparam name="T">The instance that you want to build</typeparam>
        /// <param name="enablePrivate">When true, private constructors will also be used to
        /// create mocks.</param>
        /// <returns>Mock instance with virtual and abstract members mocked</returns>
        public Mock<T> CreateMock<T>(bool enablePrivate)
            where T : class
        {
            var arguments = CreateArguments<T>(GetBindingFlags(enablePrivate));
            return new Mock<T>(_mockBehavior, arguments);
        }

        private object GetObjectFor(Type type)
        {
            var instance = _typeMap.ContainsKey(type) ? _typeMap[type] : CreateMockObjectAndStore(type);
            return instance.Value;
        }

        private Mock GetOrMakeMockFor(Type type)
        {
            if (!_typeMap.ContainsKey(type) || !_typeMap[type].IsMock)
            {
                _typeMap[type] = new MockInstance(type, _mockBehavior);
            }

            return ((MockInstance)_typeMap[type]).Mock;
        }

        private IInstance CreateMockObjectAndStore(Type type)
        {
            if (type.IsArray)
            {
                var elmType = type.GetElementType();

                var instance = new MockArrayInstance(elmType);
                if (_typeMap.ContainsKey(elmType))
                {
                    instance.Add(_typeMap[elmType]);
                }

                return _typeMap[type] = instance;
            }

            return _typeMap[type] = new MockInstance(type, _mockBehavior);
        }

        /// <summary>
        /// Adds an intance to the container.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="service"></param>
        public void Use<TService>(TService service)
        {
            _typeMap[typeof(TService)] = new RealInstance(service);
        }

        /// <summary>
        /// Adds an intance to the container.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="mockedService"></param>
        public void Use<TService>(Mock<TService> mockedService)
            where TService : class
        {
            _typeMap[typeof(TService)] = new MockInstance(mockedService);
        }

        /// <summary>
        /// Adds a mock object to the container that implements TService.
        /// </summary>
        /// <typeparam name="TService">The type that the instance will be registered as</typeparam>
        /// <param name="setup">A shortcut for Mock.Of's syntax</param>
        public void Use<TService>(Expression<Func<TService, bool>> setup)
            where TService : class
        {
            Use(Mock.Get(Mock.Of(setup)));
        }

        /// <summary>
        /// Searches and retrieves an object from the container that matches TService. This can be
        /// a service setup explicitly via `.Use()` or implicitly with `.CreateInstance()`.
        /// </summary>
        /// <typeparam name="TService">The class or interface to search on</typeparam>
        /// <returns>The object that implements TService</returns>
        public TService Get<TService>()
        {
            IInstance instance;
            if (!_typeMap.TryGetValue(typeof(TService), out instance))
            {
                instance = CreateMockObjectAndStore(typeof(TService));
            }

            return (TService)_typeMap[typeof(TService)].Value;
        }

        /// <summary>
        /// Searches and retrieves the mock that the container uses for TService.
        /// </summary>
        /// <typeparam name="TService">The class or interface to search on</typeparam>
        /// <exception cref="ArgumentException">if the requested object wasn't a Mock</exception>
        /// <returns>a mock that </returns>
        public Mock<TService> GetMock<TService>()
            where TService : class
        {
            IInstance instance;
            if (!_typeMap.TryGetValue(typeof(TService), out instance))
            {
                instance = CreateMockObjectAndStore(typeof(TService));
            }

            if (!instance.IsMock)
            {
                throw new ArgumentException($"Registered service `{Get<TService>().GetType()}` was not a mock");
            }

            var mockInstance = (MockInstance)instance;
            return (Mock<TService>)mockInstance.Mock;
        }

        /// <summary>
        /// This is a shortcut for calling `mock.VerifyAll()` on every mock that we have.
        /// </summary>
        public void VerifyAll()
        {
            foreach (var pair in _typeMap)
            {
                if (pair.Value.IsMock)
                {
                    ((MockInstance)pair.Value).Mock.VerifyAll();
                }
            }
        }

        /// <summary>
        /// This is a shortcut for calling `mock.Verify()` on every mock that we have.
        /// </summary>
        public void Verify()
        {
            foreach (var pair in _typeMap)
            {
                if (pair.Value.IsMock)
                {
                    ((MockInstance)pair.Value).Mock.Verify();
                }
            }
        }

        /// <summary>
        /// Shortcut for mock.Setup(...), creating the mock when necessary.
        /// </summary>
        public ISetup<TService, object> Setup<TService>(Expression<Func<TService, object>> setup)
            where TService : class
        {
            Func<Mock<TService>, ISetup<TService, object>> func = m => m.Setup(setup);
            Expression<Func<Mock<TService>, ISetup<TService, object>>> expression = m => m.Setup(setup);

            // check if Func results in a cast to object (boxing). If so then the user should have used the Setup overload that
            // specifies TReturn for value types
            if (CastChecker.DoesContainCastToObject(expression))
            {
                throw new NotSupportedException("Use the Setup overload that allows specifying TReturn if the setup returns a value type");
            }

            return Setup<ISetup<TService, object>, TService>(func);
        }

        /// <summary>
        /// Shortcut for mock.Setup(...), creating the mock when necessary.
        /// </summary>
        public ISetup<TService> Setup<TService>(Expression<Action<TService>> setup)
            where TService : class
        {
            return Setup<ISetup<TService>, TService>(m => m.Setup(setup));
        }

        /// <summary>
        /// Shortcut for mock.Setup(...), creating the mock when necessary.
        /// For specific return types. E.g. primitive, structs
        /// that cannot be infered
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="setup"></param>
        /// <returns></returns>
        public ISetup<TService, TReturn> Setup<TService, TReturn>(Expression<Func<TService, TReturn>> setup)
            where TService : class
        {
            return Setup<ISetup<TService, TReturn>, TService>(m => m.Setup(setup));
        }

        private TReturn Setup<TReturn, TService>(Func<Mock<TService>, TReturn> returnValue)
            where TService : class
        {
            var mock = (Mock<TService>)GetOrMakeMockFor(typeof(TService));
            Use(mock);
            return returnValue(mock);
        }

        /// <summary>
        /// Shortcut for mock.SetupAllProperties(), creating the mock when necessary
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        public Mock<TService> SetupAllProperties<TService>()
            where TService : class
        {
            var mock = (Mock<TService>)GetOrMakeMockFor(typeof(TService));
            Use(mock);
            mock.SetupAllProperties();
            return mock;
        }

        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to
        /// other interfaces. In the end, this just means that all given
        /// types will be implemnted by the same instance.
        /// </summary>
        public void Combine(Type type, params Type[] forwardTo)
        {
            var mockObject = new MockInstance(type, _mockBehavior);
            forwardTo.Aggregate(mockObject.Mock, As);

            foreach (var serviceType in forwardTo.Concat(new[] { type }))
            {
                _typeMap[serviceType] = mockObject;
            }
        }

        private static Mock As(Mock mock, Type forInterface)
        {
            var genericMethodDef = mock.GetType().GetMethods().Where(x => x.Name == "As");
            var method = genericMethodDef.First().MakeGenericMethod(forInterface);
            return (Mock)method.Invoke(mock, null);
        }

        /// <summary>
        /// Verify a mock in the container.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <typeparam name="TResult">Return type of the full expression</typeparam>
        /// <param name="expression"></param>
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            mock.Verify(expression);
        }

        /// <summary>
        /// Verify a mock in the container.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <typeparam name="TResult">Return type of the full expression</typeparam>
        /// <param name="expression"></param>
        /// <param name="times"></param>
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Times times)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            mock.Verify(expression, times);
        }

        /// <summary>
        /// Verify a mock in the container.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <typeparam name="TResult">Return type of the full expression</typeparam>
        /// <param name="expression"></param>
        /// <param name="times"></param>
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Func<Times> times)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();

            mock.Verify(expression, times);
        }

        /// <summary>
        /// Verify a mock in the container.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <typeparam name="TResult">Return type of the full expression</typeparam>
        /// <param name="expression"></param>
        /// <param name="failMessage"></param>
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, string failMessage)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            mock.Verify(expression, failMessage);
        }

        /// <summary>
        /// Verify a mock in the container.
        /// </summary>
        /// <typeparam name="T">Type of the mock</typeparam>
        /// <typeparam name="TResult">Return type of the full expression</typeparam>
        /// <param name="expression"></param>
        /// <param name="times"></param>
        /// <param name="failMessage"></param>
        public void Verify<T, TResult>(Expression<Func<T, TResult>> expression, Times times, string failMessage)
            where T : class
            where TResult : struct
        {
            var mock = GetMock<T>();
            mock.Verify(expression, times, failMessage);
        }
    }
}