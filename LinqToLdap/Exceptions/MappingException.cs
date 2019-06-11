using System;

namespace LinqToLdap.Exceptions
{
    /// <summary>
    /// Exception to indicate that something has been mapped incorrectly or not at all.
    /// </summary>
    public class MappingException : Exception
    {
        internal MappingException(string message) : base(message)
        {
        }

        internal MappingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}