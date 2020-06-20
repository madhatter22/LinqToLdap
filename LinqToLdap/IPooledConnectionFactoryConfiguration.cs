using System;
using System.DirectoryServices.Protocols;
using System.Net;

namespace LinqToLdap
{
    /// <summary>
    /// Interface for configuring a <see cref="ILdapConnectionFactory"/> that supports connection pooling.
    /// </summary>
    public interface IPooledConnectionFactoryConfiguration
    {
        /// <summary>
        /// Allows you to specify an authentication method for the
        /// connection.  If this method is not called,  the authentication method
        /// will be resolved by the <see cref="LdapConnection"/>.
        /// </summary>
        /// <param name="authType">
        /// The type of authentication to use.
        /// </param>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration AuthenticateBy(AuthType authType);

        /// <summary>
        /// Allows you to specify credentials for the connection to use.
        /// If this method is not called,  then the <see cref="LdapConnection"/>
        /// will use the credentials of the current user.
        /// </summary>
        /// <param name="credentials">
        /// The credentials to use.
        /// </param>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration AuthenticateAs(NetworkCredential credentials);

        /// <summary>
        /// Specifies the LDAP protocol version.  The default is 3.
        /// </summary>
        /// <param name="version">The protocol version</param>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration ProtocolVersion(int version);

        /// <summary>
        /// Sets the connection timeout period in seconds.  The default is 30 seconds.
        /// </summary>
        /// <param name="seconds">The timeout period</param>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="seconds"/> is less than or equal to 0</exception>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration ConnectionTimeoutIn(double seconds);

        /// <summary>
        /// Sets the port manually for the LDAP server.  The default is 389.
        /// </summary>
        /// <param name="port">
        /// The port to use when communicating with the LDAP server.
        /// </param>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration UsePort(int port);

        /// <summary>
        /// Enables SSL with a specific port.
        /// </summary>
        /// <param name="port">The optional port to use.</param>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration UseSsl(int port);

        /// <summary>
        /// Enables SSL with a default port of 636.
        /// </summary>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration UseSsl();

        /// <summary>
        /// If this option is called, the server name is a fully-qualified DNS host name.
        /// Otherwise the server name can be an IP address, a DNS domain or host name.
        /// </summary>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration ServerNameIsFullyQualified();

        /// <summary>
        /// Indicates that the connections will use UDP (User Datagram Protocol).
        /// </summary>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration UseUdp();

        /// <summary>
        /// The maximum number of connections the pool supports. An <see cref="InvalidOperationException"/> is thrown if the pool size is exceeded.
        /// Default is 50.
        /// </summary>
        /// <param name="size">The value</param>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration MaxPoolSizeIs(int size);

        /// <summary>
        /// The minimum number of connections the pool will maintain. Default is 0.
        /// </summary>
        /// <param name="size">The value</param>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration MinPoolSizeIs(int size);

        /// <summary>
        /// Configure the max age of any connection in the pool.
        /// </summary>
        /// <param name="timeSpan">The timespan</param>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration MaxConnectionAgeIs(TimeSpan timeSpan);

        /// <summary>
        /// The time in minutes before a connection is considered stale and ready for scavenging. Default is 1 minute.
        /// </summary>
        /// <param name="idleTime">The value</param>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration ConnectionIdleTimeIs(double idleTime);

        /// <summary>
        /// Time in milliseconds for how often the factory will try to clean up excess connections. Default is 90 seconds.
        /// </summary>
        /// <param name="interval">The value</param>
        /// <returns></returns>
        IPooledConnectionFactoryConfiguration ScavengeIntervalIs(double interval);
    }
}