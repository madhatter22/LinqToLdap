using System;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.Logging
{
    /// <summary>
    /// Interface for logging <see cref="DirectoryRequest"/> and <see cref="DirectoryResponse"/> information.
    /// </summary>
    public interface ILinqToLdapLogger
    {
        /// <summary>
        /// Allows changing and checking if trace logging is enabled.
        /// </summary>
        bool TraceEnabled { get; set; }

        /// <summary>
        /// Logs query information if <see cref="TraceEnabled"/> is true.
        /// </summary>
        /// <param name="message"></param>
        void Trace(string message);

        /// <summary>
        /// Writes the optional message and exception to the log.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        void Error(Exception ex, string message = null);
    }
}