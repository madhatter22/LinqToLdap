using System;

namespace LinqToLdap.Exceptions
{
    /// <summary>
    /// Exception thrown if delegate creation fails for mapped objects
    /// </summary>
    public class MissingConstructorException : Exception
    {
        internal MissingConstructorException(string message) : base(message)
        {
        }

        internal MissingConstructorException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}