namespace NuGet
{
    using System;
    using System.Reflection;

    internal static class ExceptionUtility
    {
        public static Exception Unwrap(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            return ((exception.InnerException != null) ? (((exception is AggregateException) || (exception is TargetInvocationException)) ? exception.GetBaseException() : exception) : exception);
        }
    }
}

