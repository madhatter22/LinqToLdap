/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.DirectoryServices.Protocols;
using System.Net;

namespace LinqToLdap
{
    /// <summary>
    /// Class for creating <see cref="LdapConnection"/>s using a fluent configuration.
    /// </summary>
    public class LdapConnectionFactory : ConnectionFactoryBase, ILdapConnectionFactoryConfiguration, ILdapConnectionFactory
    {
        /// <summary>
        /// Allows for building an <see cref="LdapConnection"/> via a fluent interface.
        /// </summary>
        /// <param name="serverName">The name of the server as 'servername' or 'servername:port'</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="serverName"/> is null, empty, or white-space</exception>
        public LdapConnectionFactory(string serverName) : base(serverName)
        {
        }

        /// <summary>
        /// Specifies the LDAP protocol version.  The default is 3.
        /// </summary>
        /// <param name="version">The protocol version</param>
        /// <returns></returns>
        public ILdapConnectionFactoryConfiguration ProtocolVersion(int version)
        {
            LdapProtocolVersion = version;
            return this;
        }

        /// <summary>
        /// Sets the port manually for the LDAP server.  The default is 389.
        /// </summary>
        /// <param name="port">
        /// The port to use when communicating with the LDAP server.
        /// </param>
        /// <returns></returns>
        public ILdapConnectionFactoryConfiguration UsePort(int port)
        {
            Port = port;
            return this;
        }

        /// <summary>
        /// Sets the connection timeout period in seconds.  The default is 30 seconds.
        /// </summary>
        /// <param name="seconds">The timeout period</param>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="seconds"/> is less than or equal to 0</exception>
        /// <returns></returns>
        public ILdapConnectionFactoryConfiguration ConnectionTimeoutIn(double seconds)
        {
            if (seconds <= 0) throw new ArgumentException("seconds must be greater than 0");
            Timeout = TimeSpan.FromSeconds(seconds);

            return this;
        }

        /// <summary>
        /// Sets the port to the default SSL port for the LDAP server.  The default is 636.
        /// </summary>
        /// <returns></returns>
        public ILdapConnectionFactoryConfiguration UseSsl(int port = SslPort)
        {
            UsesSsl = true;
            UsePort(port);
            return this;
        }

        /// <summary>
        /// If this option is called, the server name is a fully-qualified DNS host name. 
        /// Otherwise the server name can be an IP address, a DNS domain or host name.
        /// </summary>
        /// <returns></returns>
        public ILdapConnectionFactoryConfiguration ServerNameIsFullyQualified()
        {
            FullyQualifiedDnsHostName = true;
            return this;
        }

        /// <summary>
        /// Indicates that the connections will use UDP (User Datagram Protocol).
        /// </summary>
        /// <returns></returns>
        public ILdapConnectionFactoryConfiguration UseUdp()
        {
            IsConnectionless = true;
            return this;
        }

        /// <summary>
        /// Allows you to specify an authentication method for the 
        /// connection.  If this method is not called,  the authentication method 
        /// will be resolved by the <see cref="LdapConnection"/>.
        /// </summary>
        /// <param name="authType">
        /// The type of authentication to use.
        /// </param>
        /// <returns></returns>
        public ILdapConnectionFactoryConfiguration AuthenticateBy(AuthType authType)
        {
            AuthType = authType;
            return this;
        }

        /// <summary>
        /// Allows you to specify credentials for the connection to use.  
        /// If this method is not called,  then the <see cref="LdapConnection"/> 
        /// will use the credentials of the current user.
        /// </summary>
        /// <param name="credentials">
        /// The credentials to use.
        /// </param>
        /// <returns></returns>
        public ILdapConnectionFactoryConfiguration AuthenticateAs(NetworkCredential credentials)
        {
            Credentials = credentials;
            return this;
        }

        /// <summary>
        /// Releases a <see cref="LdapConnection"/>.
        /// </summary>
        /// <returns></returns>
        public void ReleaseConnection(LdapConnection connection)
        {
            connection.Dispose();
            if (Logger != null && Logger.TraceEnabled) Logger.Trace("Connection Disposed");
        }

        /// <summary>
        /// Builds a <see cref="LdapConnection"/> based on a fluent configuration.
        /// </summary>
        /// <returns></returns>
        public LdapConnection GetConnection()
        {
            try
            {
                return BuildConnection();
            }
            catch (Exception ex)
            {
                if (Logger != null) Logger.Error(ex);
                throw;
            }
        }
    }
}
