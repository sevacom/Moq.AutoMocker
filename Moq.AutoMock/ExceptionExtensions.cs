using System;
using System.Runtime.ExceptionServices;

namespace Moq.AutoMock
{
    internal static class ExceptionExtensions
    {
        /// <summary>
        /// Preserve the stack trance on an exception so it can be rethrown without losing that information
        /// </summary>
        /// <param name="ex">The exception to preserve the stack trace on</param>
        public static Exception PreserveStackTrace(this Exception ex)
        {
            // If switching to .NET4.5+, the following line is a better method to use.
            ExceptionDispatchInfo.Capture(ex).Throw();
            return ex;
        }
    }
}
