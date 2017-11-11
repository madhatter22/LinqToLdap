/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System.DirectoryServices.Protocols;

namespace LinqToLdap
{
    /// <summary>
    /// Interface for constructing <see cref="LdapConnection"/>s and managing them in a pool.
    /// </summary>
    public interface IPooledLdapConnectionFactory : ILdapConnectionFactory
    {
        /// <summary>
        /// Reinitializes the pool.
        /// </summary>
        void Reinitialize();
    }
}