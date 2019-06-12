using System;
using System.DirectoryServices.Protocols;
using System.Net;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests
{
    [TestClass]
    public class ConnectionFactoryTest : ConnectionFactoryBase
    {
        public ConnectionFactoryTest() : base("localhost")
        {
        }

        public ConnectionFactoryTest(string serverName) : base(serverName)
        {
        }

        [TestMethod]
        public void Constructor_NullServerName_ThrowsArgumentNullException()
        {
            Executing.This(() => new ConnectionFactoryTest(null)).Should()
                .Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_BasicServerName_SetsServerNameAndUsesDefaultPort()
        {
            //act
            var factory = new ConnectionFactoryTest("server");

            //assert
            factory.ServerName.Should().Be.EqualTo("server");
            factory.Port.Should().Be.EqualTo(389);
        }

        [TestMethod]
        public void Constructor_ServerNameWithProtocol_StripsProtocol()
        {
            //act
            var factory = new ConnectionFactoryTest("Ldap://server:1234/");

            //assert
            factory.ServerName.Should().Be.EqualTo("server");
            factory.Port.Should().Be.EqualTo(1234);
        }

        [TestMethod]
        public void Constructor_ServerNameWithProtocolAndDistinguishedName_ThrowsArgumentException()
        {
            //assert
            Executing.This(() => new ConnectionFactoryTest("LDAP://server:1234/DC=local,DC=com"))
                .Should().Throw<ArgumentException>()
                .And.Exception.Message.Should().Be.EqualTo("You cannot specify a directory container in the connection.  Use only \"server:1234\" and then use DirectoryContext to query \"dc=local,dc=com\".");
        }

        [TestMethod]
        public void Constructor_ServerNameAndPort389_SetsServerNameAndUsesDefaultPort()
        {
            //act
            var factory = new ConnectionFactoryTest("server:389");

            //assert
            factory.ServerName.Should().Be.EqualTo("server");
            factory.Port.Should().Be.EqualTo(389);
        }

        [TestMethod]
        public void Constructor_ServerNameAndPort636_SetsServerNameAndUsesSsl()
        {
            //act
            var factory = new ConnectionFactoryTest("server:636");

            //assert
            factory.ServerName.Should().Be.EqualTo("server");
            factory.Port.Should().Be.EqualTo(636);
            factory.UsesSsl.Should().Be.True();
        }

        [TestMethod]
        public void Constructor_ServerNameAndPort3268_SetsServerNameAndUsesGlobalCatalog()
        {
            //act
            var factory = new ConnectionFactoryTest("server:3268");

            //assert
            factory.ServerName.Should().Be.EqualTo("server");
            factory.Port.Should().Be.EqualTo(3268);
        }

        [TestMethod]
        public void Constructor_ServerNameAndAnyPort_SetsServerNameAndUsesPort()
        {
            //act
            var factory = new ConnectionFactoryTest("server:1111");

            //assert
            factory.ServerName.Should().Be.EqualTo("server");
            factory.Port.Should().Be.EqualTo(1111);
        }

        [TestMethod]
        public void Constructor_ServerNameAndUnknownFormat_SetsServerName()
        {
            //act
            var factory = new ConnectionFactoryTest("server:1111:asd");

            //assert
            factory.ServerName.Should().Be.EqualTo("server:1111:asd");
            factory.Port.Should().Be.EqualTo(389);
        }

        [TestMethod]
        public void GetConnection_NoAuthType_BuildsConnectionWithoutAuthType()
        {
            //preapre
            Credentials = CredentialCache.DefaultNetworkCredentials;
            IsConnectionless = true;

            //act
            var connection = BuildConnection();

            //assert
            connection.SessionOptions.ProtocolVersion.Should().Be.EqualTo(3);
            connection.SessionOptions.SecureSocketLayer.Should().Be.False();
#if (NET35 || NET40 || NET45)
            connection.FieldValueEx<NetworkCredential>("directoryCredential").Should().Not.Be.Null();
            connection.FieldValueEx<LdapDirectoryIdentifier>("directoryIdentifier").Connectionless.Should().Be.True();
            connection.FieldValueEx<LdapDirectoryIdentifier>("directoryIdentifier").FullyQualifiedDnsHostName.Should().Be.False();
            connection.FieldValueEx<LdapDirectoryIdentifier>("directoryIdentifier").PortNumber.Should().Be.EqualTo(389);
#else
            connection.FieldValueEx<NetworkCredential>("_directoryCredential").Should().Not.Be.Null();
            connection.FieldValueEx<LdapDirectoryIdentifier>("_directoryIdentifier").Connectionless.Should().Be.True();
            connection.FieldValueEx<LdapDirectoryIdentifier>("_directoryIdentifier").FullyQualifiedDnsHostName.Should().Be.False();
            connection.FieldValueEx<LdapDirectoryIdentifier>("_directoryIdentifier").PortNumber.Should().Be.EqualTo(389);
#endif
        }

        [TestMethod]
        public void GetConnection_AuthType_BuildsConnectionWithoutAuthType()
        {
            //preapre
            IsConnectionless = true;
            LdapProtocolVersion = 2;
            AuthType = System.DirectoryServices.Protocols.AuthType.Basic;

            //act
            var connection = BuildConnection();

            //assert
            connection.SessionOptions.ProtocolVersion.Should().Be.EqualTo(2);
            connection.SessionOptions.SecureSocketLayer.Should().Be.False();
#if (NET35 || NET40 || NET45)
            connection.FieldValueEx<AuthType>("connectionAuthType").Should().Be.EqualTo(System.DirectoryServices.Protocols.AuthType.Basic);
            connection.FieldValueEx<NetworkCredential>("directoryCredential").Should().Be.Null();
            connection.FieldValueEx<LdapDirectoryIdentifier>("directoryIdentifier").Connectionless.Should().Be.True();
            connection.FieldValueEx<LdapDirectoryIdentifier>("directoryIdentifier").FullyQualifiedDnsHostName.Should().Be.False();
            connection.FieldValueEx<LdapDirectoryIdentifier>("directoryIdentifier").PortNumber.Should().Be.EqualTo(389);
#else
            connection.AuthType.Should().Be.EqualTo(System.DirectoryServices.Protocols.AuthType.Basic);
            connection.FieldValueEx<NetworkCredential>("_directoryCredential").Should().Be.Null();
            connection.FieldValueEx<LdapDirectoryIdentifier>("_directoryIdentifier").Connectionless.Should().Be.True();
            connection.FieldValueEx<LdapDirectoryIdentifier>("_directoryIdentifier").FullyQualifiedDnsHostName.Should().Be.False();
            connection.FieldValueEx<LdapDirectoryIdentifier>("_directoryIdentifier").PortNumber.Should().Be.EqualTo(389);
#endif
        }
    }
}