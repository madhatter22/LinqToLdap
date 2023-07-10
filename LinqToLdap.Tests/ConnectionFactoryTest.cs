﻿using System;
using System.DirectoryServices.Protocols;
using System.Net;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

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
            factory.ServerName.Should().Be("server");
            factory.Port.Should().Be(389);
        }

        [TestMethod]
        public void Constructor_ServerNameWithProtocol_StripsProtocol()
        {
            //act
            var factory = new ConnectionFactoryTest("Ldap://server:1234/");

            //assert
            factory.ServerName.Should().Be("server");
            factory.Port.Should().Be(1234);
        }

        [TestMethod]
        public void Constructor_ServerNameWithProtocolAndDistinguishedName_ThrowsArgumentException()
        {
            //assert
            Executing.This(() => new ConnectionFactoryTest("LDAP://server:1234/DC=local,DC=com"))
                .Should().Throw<ArgumentException>()
                .And.Message.Should().Be("You cannot specify a directory container in the connection.  Use only \"server:1234\" and then use DirectoryContext to query \"dc=local,dc=com\".");
        }

        [TestMethod]
        public void Constructor_ServerNameAndPort389_SetsServerNameAndUsesDefaultPort()
        {
            //act
            var factory = new ConnectionFactoryTest("server:389");

            //assert
            factory.ServerName.Should().Be("server");
            factory.Port.Should().Be(389);
        }

        [TestMethod]
        public void Constructor_ServerNameAndPort636_SetsServerNameAndUsesSsl()
        {
            //act
            var factory = new ConnectionFactoryTest("server:636");

            //assert
            factory.ServerName.Should().Be("server");
            factory.Port.Should().Be(636);
            factory.UsesSsl.Should().BeTrue();
        }

        [TestMethod]
        public void Constructor_ServerNameAndPort3268_SetsServerNameAndUsesGlobalCatalog()
        {
            //act
            var factory = new ConnectionFactoryTest("server:3268");

            //assert
            factory.ServerName.Should().Be("server");
            factory.Port.Should().Be(3268);
        }

        [TestMethod]
        public void Constructor_ServerNameAndAnyPort_SetsServerNameAndUsesPort()
        {
            //act
            var factory = new ConnectionFactoryTest("server:1111");

            //assert
            factory.ServerName.Should().Be("server");
            factory.Port.Should().Be(1111);
        }

        [TestMethod]
        public void Constructor_ServerNameAndUnknownFormat_SetsServerName()
        {
            //act
            var factory = new ConnectionFactoryTest("server:1111:asd");

            //assert
            factory.ServerName.Should().Be("server:1111:asd");
            factory.Port.Should().Be(389);
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
            connection.SessionOptions.ProtocolVersion.Should().Be(3);
            connection.SessionOptions.SecureSocketLayer.Should().BeFalse();
            connection.FieldValueEx<NetworkCredential>("_directoryCredential").Should().NotBeNull();
            connection.FieldValueEx<LdapDirectoryIdentifier>("_directoryIdentifier").Connectionless.Should().BeTrue();
            connection.FieldValueEx<LdapDirectoryIdentifier>("_directoryIdentifier").FullyQualifiedDnsHostName.Should().BeFalse();
            connection.FieldValueEx<LdapDirectoryIdentifier>("_directoryIdentifier").PortNumber.Should().Be(389);
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
            connection.SessionOptions.ProtocolVersion.Should().Be(2);
            connection.SessionOptions.SecureSocketLayer.Should().BeFalse();
            connection.AuthType.Should().Be(System.DirectoryServices.Protocols.AuthType.Basic);
            connection.FieldValueEx<NetworkCredential>("_directoryCredential").Should().BeNull();
            connection.FieldValueEx<LdapDirectoryIdentifier>("_directoryIdentifier").Connectionless.Should().BeTrue();
            connection.FieldValueEx<LdapDirectoryIdentifier>("_directoryIdentifier").FullyQualifiedDnsHostName.Should().BeFalse();
            connection.FieldValueEx<LdapDirectoryIdentifier>("_directoryIdentifier").PortNumber.Should().Be(389);
        }
    }
}