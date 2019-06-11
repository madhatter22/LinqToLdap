using LinqToLdap.Logging;
using System;
using System.DirectoryServices.Protocols;
using System.Net;

namespace LinqToLdap
{
    /// <summary>
    /// Base class for connection factories.
    /// </summary>
    public abstract class ConnectionFactoryBase
    {
        /// <summary>
        /// Default LDAP port 389
        /// </summary>
        public const int DefaultPort = 389;

        /// <summary>
        /// Default LDAP SSL port 636
        /// </summary>
        public const int SslPort = 636;

        /// <summary>
        /// Default Active Directory port 3268
        /// </summary>
        public const int GlobalCatalogPort = 3268;

        /// <summary>
        /// Default Active Directory port 3269
        /// </summary>
        public const int GlobalCatalogSslPort = 3269;

        /// <summary>
        /// Default LDAP protocol version 3
        /// </summary>
        public const int DefaultProtocolVersion = 3;

        /// <summary>
        /// Sets the server name and port if one was specified as part of the server name.
        /// </summary>
        /// <param name="serverName"></param>
        protected ConnectionFactoryBase(string serverName)
        {
            if (serverName.IsNullOrEmpty()) throw new ArgumentNullException("serverName");

            if (serverName.StartsWith("ldap://", StringComparison.OrdinalIgnoreCase))
            {
                serverName = serverName.ToLower().Replace("ldap://", "");

                var preServerInfo = serverName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                switch (preServerInfo.Length)
                {
                    case 1:
                        serverName = preServerInfo[0];
                        break;

                    case 2:
                        throw new ArgumentException(
                            string.Format(
                                "You cannot specify a directory container in the connection.  Use only \"{0}\" and then use DirectoryContext to query \"{1}\".",
                                preServerInfo[0], preServerInfo[1]));
                }
            }

            var serverInfo = serverName.Split(':');

            Port = DefaultPort;
            switch (serverInfo.Length)
            {
                case 1:
                    ServerName = serverInfo[0];
                    break;

                case 2:
                    {
                        int port;
                        if (int.TryParse(serverInfo[1], out port))
                        {
                            ServerName = serverInfo[0];
                            switch (port)
                            {
                                case DefaultPort:
                                    break;

                                case SslPort:
                                    UsesSsl = true;
                                    Port = port;
                                    break;

                                case GlobalCatalogPort:
                                    Port = port;
                                    break;

                                case GlobalCatalogSslPort:
                                    UsesSsl = true;
                                    Port = port;
                                    break;

                                default:
                                    Port = port;
                                    break;
                            }
                        }
                        else
                        {
                            ServerName = serverName;
                        }
                    }
                    break;

                default:
                    ServerName = serverName;
                    break;
            }

            LdapProtocolVersion = DefaultProtocolVersion;
            Timeout = new TimeSpan(0, 0, 30);
        }

        /// <summary>
        /// Constructs a connection from the fluent configuration.
        /// </summary>
        /// <returns></returns>
        protected virtual LdapConnection BuildConnection()
        {
            if (Logger != null && Logger.TraceEnabled) Logger.Trace("Building Connection");
            try
            {
                var identifier = new LdapDirectoryIdentifier(ServerName, Port,
                FullyQualifiedDnsHostName, IsConnectionless);

                var connection = AuthType.HasValue
                    ? new LdapConnection(identifier, Credentials, AuthType.Value)
                    : new LdapConnection(identifier, Credentials);

                connection.SessionOptions.ProtocolVersion = LdapProtocolVersion;
                connection.SessionOptions.SecureSocketLayer = UsesSsl;
                connection.Timeout = Timeout;

                if (Logger != null && Logger.TraceEnabled) Logger.Trace("Connection Built");

                return connection;
            }
            catch (Exception ex)
            {
                if (Logger != null) Logger.Error(ex);

                throw;
            }
        }

        /// <summary>
        /// Indicates if the connection uses SSL.  Will automatically set the port to 636.
        /// </summary>
        protected bool UsesSsl { get; set; }

        /// <summary>
        /// Credentials for the connection
        /// </summary>
        protected NetworkCredential Credentials { get; set; }

        /// <summary>
        /// LDAP protocol version
        /// </summary>
        protected int LdapProtocolVersion { get; set; }

        /// <summary>
        /// If the connection should use UDP
        /// </summary>
        protected bool IsConnectionless { get; set; }

        /// <summary>
        /// <see cref="AuthType"/> for the connection.
        /// </summary>
        protected AuthType? AuthType { get; set; }

        /// <summary>
        /// Timeout period for the connection.
        /// </summary>
        protected TimeSpan Timeout { get; set; }

        /// <summary>
        /// If this option is called, the server name is a fully-qualified DNS host name.
        /// Otherwise the server name can be an IP address, a DNS domain or host name.
        /// </summary>
        /// <returns></returns>
        protected bool FullyQualifiedDnsHostName { get; set; }

        /// <summary>
        /// Port for the connection
        /// </summary>
        protected int Port { get; set; }

        /// <summary>
        /// Name of the server
        /// </summary>
        protected string ServerName { get; set; }

        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        public ILinqToLdapLogger Logger { get; set; }
    }
}