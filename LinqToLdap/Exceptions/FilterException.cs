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
