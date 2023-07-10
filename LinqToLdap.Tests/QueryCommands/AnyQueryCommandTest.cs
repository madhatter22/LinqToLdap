﻿using System.DirectoryServices.Protocols;
using System.Xml;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands;
using LinqToLdap.QueryCommands.Options;
using LinqToLdap.Tests.TestSupport;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.QueryCommands
{
    [TestClass]
    public class AnyQueryCommandTest
    {
        [TestMethod]
        public void Execute_NoEntries_ReturnsFalse()
        {
            //prepare
            var options = new Mock<IQueryCommandOptions>();
            var connection = new MockLdapConnection();
            var xmlNode = new XmlDocument();
            var mapping = new Mock<IObjectMapping>();
            mapping.Setup(m => m.NamingContext)
                .Returns("nm");
            var command = new AnyQueryCommand(options.Object, mapping.Object);
            object[] parameters;
#if (!NET35 && !NET40 && !NET45)
            parameters = new object[] { "", new System.DirectoryServices.Protocols.DirectoryControl[0], System.DirectoryServices.Protocols.ResultCode.Success, "", new System.Uri[0] };
#else
            parameters = new object[] { xmlNode };
#endif
            var response = typeof(SearchResponse).Create<SearchResponse>(parameters);
#if (NET35 || NET40 || NET45)
            response.SetFieldValue("result", ResultCode.Success);
#endif

            connection.RequestResponses.Add(typeof(SearchRequest), response);

            //act
            var result = command.Execute(connection, SearchScope.Subtree, 1, true);

            //assert
            var request = connection.SentRequests[0];
            request.Controls[0].CastTo<PageResultRequestControl>().PageSize.Should().Be(1);
            request.CastTo<SearchRequest>().Scope.Should().Be(SearchScope.Subtree);
            request.CastTo<SearchRequest>().Attributes.Count.Should().Be(1);
            request.CastTo<SearchRequest>().Attributes.Contains("cn").Should().BeTrue();
            request.CastTo<SearchRequest>().TypesOnly.Should().BeTrue();

            result.Should().Be(false);
        }

        [TestMethod]
        public void Execute_PagingDisabled_HasNoPageResultRequestControl()
        {
            //prepare
            var options = new Mock<IQueryCommandOptions>();
            var connection = new MockLdapConnection();
            var xmlNode = new XmlDocument();
            var mapping = new Mock<IObjectMapping>();
            mapping.Setup(m => m.NamingContext)
                .Returns("nm");
            var command = new AnyQueryCommand(options.Object, mapping.Object);
            object[] parameters;
#if (!NET35 && !NET40 && !NET45)
            parameters = new object[] { "", new System.DirectoryServices.Protocols.DirectoryControl[0], System.DirectoryServices.Protocols.ResultCode.Success, "", new System.Uri[0] };
#else
            parameters = new object[] { xmlNode };
#endif
            var response = typeof(SearchResponse).Create<SearchResponse>(parameters);
#if (NET35 || NET40 || NET45)
            response.SetFieldValue("result", ResultCode.Success);
#endif
            connection.RequestResponses.Add(typeof(SearchRequest), response);

            //act
            var result = command.Execute(connection, SearchScope.Subtree, 1, false);

            //assert
            var request = connection.SentRequests[0];
            request.Controls.Count.Should().Be(0);
            request.CastTo<SearchRequest>().Scope.Should().Be(SearchScope.Subtree);
            request.CastTo<SearchRequest>().Attributes.Count.Should().Be(1);
            request.CastTo<SearchRequest>().Attributes.Contains("cn").Should().BeTrue();
            request.CastTo<SearchRequest>().TypesOnly.Should().BeTrue();

            result.Should().Be(false);
        }

        [TestMethod]
        public void Execute_OneEntry_ReturnsTrue()
        {
            //prepare
            var options = new Mock<IQueryCommandOptions>();
            var connection = new MockLdapConnection();
            var xmlNode = new XmlDocument();
            var mapping = new Mock<IObjectMapping>();
            mapping.Setup(m => m.NamingContext)
                .Returns("nm");
            var command = new AnyQueryCommand(options.Object, mapping.Object);
            object[] parameters;
#if (!NET35 && !NET40 && !NET45)
            parameters = new object[] { "", new System.DirectoryServices.Protocols.DirectoryControl[0], System.DirectoryServices.Protocols.ResultCode.Success, "", new System.Uri[0] };
#else
            parameters = new object[] { xmlNode };
#endif
            var response = typeof(SearchResponse).Create<SearchResponse>(parameters);
#if (NET35 || NET40 || NET45)
            response.SetFieldValue("result", ResultCode.Success);
#endif
            connection.RequestResponses.Add(typeof(SearchRequest), response);
            var collection =
                typeof(SearchResultEntryCollection).Create<SearchResultEntryCollection>();
            var entry = typeof(SearchResultEntry).Create<SearchResultEntry>("dn");
            collection.Call("Add", entry);
#if (!NET35 && !NET40 && !NET45)
            response.Call("set_Entries", collection);
#else
            response.Call("SetEntries", collection);
#endif

            //act
            var result = command.Execute(connection, SearchScope.Subtree, 1, true);

            //assert
            var request = connection.SentRequests[0];
            request.Controls[0].CastTo<PageResultRequestControl>().PageSize.Should().Be(1);
            request.CastTo<SearchRequest>().Scope.Should().Be(SearchScope.Subtree);
            request.CastTo<SearchRequest>().Attributes.Count.Should().Be(1);
            request.CastTo<SearchRequest>().Attributes.Contains("cn").Should().BeTrue();
            request.CastTo<SearchRequest>().TypesOnly.Should().BeTrue();

            result.Should().Be(true);
        }

        [TestMethod]
        public void Execute_OneEntryAndCustomNamingContext_ReturnsTrue()
        {
            //prepare
            var options = new Mock<IQueryCommandOptions>();
            var connection = new MockLdapConnection();
            var xmlNode = new XmlDocument();
            var mapping = new Mock<IObjectMapping>();
            var command = new AnyQueryCommand(options.Object, mapping.Object);
            object[] parameters;
#if (!NET35 && !NET40 && !NET45)
            parameters = new object[] { "", new System.DirectoryServices.Protocols.DirectoryControl[0], System.DirectoryServices.Protocols.ResultCode.Success, "", new System.Uri[0] };
#else
            parameters = new object[] { xmlNode };
#endif
            var response = typeof(SearchResponse).Create<SearchResponse>(parameters);
#if (NET35 || NET40 || NET45)
            response.SetFieldValue("result", ResultCode.Success);
#endif
            connection.RequestResponses.Add(typeof(SearchRequest), response);
            var collection =
                typeof(SearchResultEntryCollection).Create<SearchResultEntryCollection>();
            var entry = typeof(SearchResultEntry).Create<SearchResultEntry>("dn");
            collection.Call("Add", entry);
#if (!NET35 && !NET40 && !NET45)
            response.Call("set_Entries", collection);
#else
            response.Call("SetEntries", collection);
#endif

            //act
            var result = command.Execute(connection, SearchScope.Subtree, 1, true, null, "nc");

            //assert
            var request = connection.SentRequests[0];
            command.FieldValueEx<SearchRequest>("SearchRequest").DistinguishedName.Should().Be("nc");
            request.Controls[0].CastTo<PageResultRequestControl>().PageSize.Should().Be(1);
            request.CastTo<SearchRequest>().Scope.Should().Be(SearchScope.Subtree);
            request.CastTo<SearchRequest>().Attributes.Count.Should().Be(1);
            request.CastTo<SearchRequest>().Attributes.Contains("cn").Should().BeTrue();
            request.CastTo<SearchRequest>().TypesOnly.Should().BeTrue();

            result.Should().Be(true);
        }
    }
}