using System;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Reflection;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.ClassMapAssembly;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Mapping
{
    [TestClass]
    public class LdapConfigurationTest
    {
        [TestMethod]
        public void GetConnection_CredentialsAndAuthType_ReturnsConnectionWithCredentialsAndAnonymousAuthType()
        {
            //prepare
            var config = new LdapConfiguration();

            config.ConfigureFactory("server")
                .AuthenticateAs(CredentialCache.DefaultNetworkCredentials)
                .AuthenticateBy(AuthType.Anonymous);

            //act
            var connection = config.ConnectionFactory.GetConnection();

            //assert
            connection.AuthType.Should().Be(AuthType.Anonymous);
#if (NET35 || NET40 || NET45)
            connection.FieldValueEx<NetworkCredential>("directoryCredential")
                .Should().NotBeNull();
#endif
        }

        [TestMethod]
        public void AddMapping_AddsMapping()
        {
            //prepare
            var config = new LdapConfiguration();
            var mapper = new Mock<IDirectoryMapper>();
            var classMap = new Mock<IClassMap>();
            config.ChangeMapper("", mapper.Object);

            //act
            config.AddMapping(classMap.Object, "nc", new[] { "oc" }, false, "oc", false);

            //assert
            mapper.Verify(m => m.Map(classMap.Object, "nc", new[] { "oc" }, false, "oc", false));
        }

        [TestMethod]
        public void AddMappingFromAssembly_AssemblyName_AddsMappings()
        {
            //prepare
            var config = new LdapConfiguration();
            var mapper = new Mock<IDirectoryMapper>();
            config.ChangeMapper("", mapper.Object);

            //act
            config.AddMappingsFrom("assemblyname");

            //assert
            mapper.Verify(m => m.AddMappingsFrom("assemblyname"));
        }

        [TestMethod]
        public void AddMappingFromAssembly_Assembly_AddsMappings()
        {
            //prepare
            var config = new LdapConfiguration();
            var mapper = new Mock<IDirectoryMapper>();
            config.ChangeMapper("", mapper.Object);
            var assembly = Assembly.GetAssembly(typeof(AssemblyTestClass));

            //act
            config.AddMappingsFrom(assembly);

            //assert
            mapper.Verify(m => m.AddMappingsFrom(assembly));
        }

        [TestMethod]
        public void AddMappingFromAssembly_EmptyAssemblyName_ThrowsArgumentNullException()
        {
            //assert
            Executing.This(() => new LdapConfiguration().AddMappingsFrom(""))
                .Should().Throw<ArgumentNullException>();
        }
    }
}