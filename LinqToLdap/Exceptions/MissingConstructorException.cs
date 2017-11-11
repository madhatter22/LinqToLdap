/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

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
