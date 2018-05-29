﻿namespace Moq.AutoMock
{
    public partial class AutoMocker
    {
        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to
        /// other interfaces. In the end, this just means that all given
        /// types will be implemented by the same instance.
        /// </summary>
        public void Combine<TService, TAsWellAs1>()
        {
            Combine(typeof(TService), typeof(TAsWellAs1));
        }

        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to
        /// other interfaces. In the end, this just means that all given
        /// types will be implemented by the same instance.
        /// </summary>
        public void Combine<TService, TAsWellAs1, TAsWellAs2>()
        {
            Combine(typeof(TService), typeof(TAsWellAs1), typeof(TAsWellAs2));
        }

        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to
        /// other interfaces. In the end, this just means that all given
        /// types will be implemented by the same instance.
        /// </summary>
        public void Combine<TService, TAsWellAs1, TAsWellAs2, TAsWellAs3>()
        {
            Combine(typeof(TService), typeof(TAsWellAs1), typeof(TAsWellAs2), typeof(TAsWellAs3));
        }

        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to
        /// other interfaces. In the end, this just means that all given
        /// types will be implemented by the same instance.
        /// </summary>
        public void Combine<TService, TAsWellAs1, TAsWellAs2, TAsWellAs3, TAsWellAs4>()
        {
            Combine(typeof(TService), typeof(TAsWellAs1), typeof(TAsWellAs2), typeof(TAsWellAs3), typeof(TAsWellAs4));
        }

        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to
        /// other interfaces. In the end, this just means that all given
        /// types will be implemented by the same instance.
        /// </summary>
        public void Combine<TService, TAsWellAs1, TAsWellAs2, TAsWellAs3, TAsWellAs4, TAsWellAs5>()
        {
            Combine(typeof(TService), typeof(TAsWellAs1), typeof(TAsWellAs2), typeof(TAsWellAs3), typeof(TAsWellAs4), typeof(TAsWellAs5));
        }

        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to
        /// other interfaces. In the end, this just means that all given
        /// types will be implemented by the same instance.
        /// </summary>
        public void Combine<TService, TAsWellAs1, TAsWellAs2, TAsWellAs3, TAsWellAs4, TAsWellAs5, TAsWellAs6>()
        {
            Combine(typeof(TService), typeof(TAsWellAs1), typeof(TAsWellAs2), typeof(TAsWellAs3), typeof(TAsWellAs4), typeof(TAsWellAs5), typeof(TAsWellAs6));
        }

        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to
        /// other interfaces. In the end, this just means that all given
        /// types will be implemented by the same instance.
        /// </summary>
        public void Combine<TService, TAsWellAs1, TAsWellAs2, TAsWellAs3, TAsWellAs4, TAsWellAs5, TAsWellAs6, TAsWellAs7>()
        {
            Combine(typeof(TService), typeof(TAsWellAs1), typeof(TAsWellAs2), typeof(TAsWellAs3), typeof(TAsWellAs4), typeof(TAsWellAs5), typeof(TAsWellAs6), typeof(TAsWellAs7));
        }

        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to
        /// other interfaces. In the end, this just means that all given
        /// types will be implemented by the same instance.
        /// </summary>
        public void Combine<TService, TAsWellAs1, TAsWellAs2, TAsWellAs3, TAsWellAs4, TAsWellAs5, TAsWellAs6, TAsWellAs7, TAsWellAs8>()
        {
            Combine(typeof(TService), typeof(TAsWellAs1), typeof(TAsWellAs2), typeof(TAsWellAs3), typeof(TAsWellAs4), typeof(TAsWellAs5), typeof(TAsWellAs6), typeof(TAsWellAs7), typeof(TAsWellAs8));
        }

        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to
        /// other interfaces. In the end, this just means that all given
        /// types will be implemented by the same instance.
        /// </summary>
        public void Combine<TService, TAsWellAs1, TAsWellAs2, TAsWellAs3, TAsWellAs4, TAsWellAs5, TAsWellAs6, TAsWellAs7, TAsWellAs8, TAsWellAs9>()
        {
            Combine(typeof(TService), typeof(TAsWellAs1), typeof(TAsWellAs2), typeof(TAsWellAs3), typeof(TAsWellAs4), typeof(TAsWellAs5), typeof(TAsWellAs6), typeof(TAsWellAs7), typeof(TAsWellAs8), typeof(TAsWellAs9));
        }

        /// <summary>
        /// Combines all given types so that they are mocked by the same
        /// mock. Some IoC containers call this "Forwarding" one type to
        /// other interfaces. In the end, this just means that all given
        /// types will be implemented by the same instance.
        /// </summary>
        public void Combine<TService, TAsWellAs1, TAsWellAs2, TAsWellAs3, TAsWellAs4, TAsWellAs5, TAsWellAs6, TAsWellAs7, TAsWellAs8, TAsWellAs9, TAsWellAs10>()
        {
            Combine(typeof(TService), typeof(TAsWellAs1), typeof(TAsWellAs2), typeof(TAsWellAs3), typeof(TAsWellAs4), typeof(TAsWellAs5), typeof(TAsWellAs6), typeof(TAsWellAs7), typeof(TAsWellAs8), typeof(TAsWellAs9), typeof(TAsWellAs10));
        }
    }
}