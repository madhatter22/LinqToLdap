/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.IO;

namespace LinqToLdap.Logging
{
    /// <summary>
    /// A simple implementation of <see cref="ILinqToLdapLogger"/> that writes to a <see cref="TextWriter"/>.
    /// </summary>
    public class SimpleTextLogger : ILinqToLdapLogger
    {
#if !NET45
        private readonly WeakReference _textWriter;
#else
        private readonly WeakReference<TextWriter> _textWriter;
#endif
        /// <summary>
        /// Creates a new logger from a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="textWriter">The log destination</param>
        public SimpleTextLogger(TextWriter textWriter)
        {
#if !NET45
        _textWriter = new WeakReference(textWriter);
#else
        _textWriter = new WeakReference<TextWriter>(textWriter);
#endif
            TraceEnabled = true;
        }

        /// <summary>
        /// Creates a new logger with a <see cref="TraceTextWriter"/>.
        /// </summary>
        public SimpleTextLogger()
        {
#if !NET45
            _textWriter = new WeakReference(TraceTextWriter.Instance);
#else
            _textWriter = new WeakReference<TextWriter>(TraceTextWriter.Instance);
#endif
            TraceEnabled = true;
        }

        /// <summary>
        /// Indicates if trace logging is enabled.
        /// </summary>
        public bool TraceEnabled { get; set; }

        /// <summary>
        /// Writes the message to the trace if <see cref="TraceEnabled"/> is true.
        /// </summary>
        /// <param name="message"></param>
        public void Trace(string message)
        {
            try
            {
#if !NET45
                try
                {
                    if (!_textWriter.IsAlive) return;
                    ((TextWriter)_textWriter.Target).WriteLine(message);
                }
                catch (InvalidOperationException)
                {
                }
#else
            TextWriter target;
            if (!_textWriter.TryGetTarget(out target)) return;
            target.WriteLine(message);
#endif
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to log trace message: " + message, ex);
            }
        }

        /// <summary>
        /// Writes the optional message and exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception.</param>
        public void Error(Exception ex, string message = null)
        {
            try
            {
#if !NET45
                try
                {
                    if (!_textWriter.IsAlive) return;
                    var writer = (TextWriter)_textWriter.Target;
                    if (message != null) writer.WriteLine(message);
                    ObjectDumper.Write(ex, 0, writer);
                }
                catch (InvalidOperationException)
                {
                }
#else
            TextWriter target;
            if (!_textWriter.TryGetTarget(out target)) return;

            if (message != null) target.WriteLine(message);
            ObjectDumper.Write(ex, 0, target);
#endif
            }
            catch
            {
                throw new Exception("Unable to log exception", ex);
            }

        }
    }
}
