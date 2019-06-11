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