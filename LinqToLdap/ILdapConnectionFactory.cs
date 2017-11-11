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
    /// Interface for constructing <see cref="LdapConnection"/>s
    /// </summary>
    public interface ILdapConnectionFactory
    {
        /// <summary>
        /// Builds a <see cref="LdapConnection"/> based on a fluent configuration.
        /// </summary>
        /// <returns></returns>
        LdapConnection GetConnection();

        /// <summary>
        /// Releases a <see cref="LdapConnection"/>.
        /// </summary>
        /// <returns></returns>
        void ReleaseConnection(LdapConnection connection);
    }
}
