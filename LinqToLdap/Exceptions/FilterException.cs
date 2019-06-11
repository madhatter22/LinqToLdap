using System;

namespace LinqToLdap.Exceptions
{
    /// <summary>
    /// Exception to indicate that something went wrong with filter creation.
    /// </summary>
    public class FilterException : Exception
    {
        internal FilterException(string message) : base(message)
        {
        }

        internal FilterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}