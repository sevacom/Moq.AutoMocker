﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Moq.AutoMock
{
    internal interface IInstance
    {
        object Value { get; }

        bool IsMock { get; }
    }

    internal class MockArrayInstance : IInstance
    {
        private readonly Type type;
        private readonly List<IInstance> mocks;

        public MockArrayInstance(Type type)
        {
            this.type = type;
            mocks = new List<IInstance>();
        }

        public IEnumerable<IInstance> Mocks => mocks;

        public object Value
        {
            get
            {
                int i = 0;
                Array array = Array.CreateInstance(type, mocks.Count);
                foreach (IInstance instance in mocks)
                {
                    array.SetValue(instance.Value, i++);
                }

                return array;
            }
        }

        public bool IsMock
        {
            get
            {
                return mocks.Any(m => m.IsMock);
            }
        }

        public void Add(IInstance instance)
        {
            mocks.Add(instance);
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    internal class MockInstance : IInstance
#pragma warning restore SA1402 // File may only contain a single class
    {
        public MockInstance(Mock value)
        {
            Mock = value;
        }

        public MockInstance(Type mockType, MockBehavior mockBehavior)
            : this(CreateMockOf(mockType, mockBehavior))
        {
        }

        private static Mock CreateMockOf(Type type, MockBehavior mockBehavior)
        {
            var mockType = typeof(Mock<>).MakeGenericType(type);
            var mock = (Mock)Activator.CreateInstance(mockType, mockBehavior);
            return mock;
        }

        public object Value => Mock.Object;

        public Mock Mock { get; }

        public bool IsMock => true;
    }

#pragma warning disable SA1402 // File may only contain a single class
    internal class RealInstance : IInstance
#pragma warning restore SA1402 // File may only contain a single class
    {
        public RealInstance(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public bool IsMock => false;
    }
}
