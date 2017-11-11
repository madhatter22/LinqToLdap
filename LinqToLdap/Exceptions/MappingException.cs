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
