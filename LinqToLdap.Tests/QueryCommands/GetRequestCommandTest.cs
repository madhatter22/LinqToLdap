using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Text;
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
    public class GetRequestCommandTest
    {
        [TestMethod]
        public void Execute_ReturnsRequest()
        {
            //prepare
            var sortingOptions = new SortingOptions();
            sortingOptions.AddSort("att1", true);
            var options = new Mock<IQueryCommandOptions>();
            options.SetupGet(o => o.Filter)
                .Returns("x=1");
            options.SetupGet(o => o.AttributesToLoad)
                .Returns(new Dictionary<string, string> {{"Property1", "prop1"}, {"Property2", "prop2"}});
            options.SetupGet(o => o.SortingOptions)
                .Returns(sortingOptions);
            options.SetupGet(o => o.Controls)
                .Returns(new DirectoryControl[] {new AsqRequestControl("att")});
            options.SetupGet(o => o.TakeSize)
                .Returns(2);
            var connection = new MockLdapConnection();
            var mapping = new Mock<IObjectMapping>();
            mapping.Setup(m => m.NamingContext)
                .Returns("nm");
            var command = new GetRequestCommand(options.Object, mapping.Object);

            //act
            var result = command.Execute(connection, SearchScope.Subtree, 1, true).CastTo<SearchRequest>();

            //assert
            result.Filter.Should().Be("x=1");
            result.Controls[0].CastTo<AsqRequestControl>().AttributeName.Should().Be("att");
            result.Controls[1].CastTo<SortRequestControl>().SortKeys[0].AttributeName.Should().Be("att1");
            result.Controls[1].CastTo<SortRequestControl>().SortKeys[0].ReverseOrder.Should().BeTrue();
            result.Controls[2].CastTo<PageResultRequestControl>().PageSize.Should().Be(2);
            result.Attributes[0].Should().Be("prop1");
            result.Attributes[1].Should().Be("prop2");
            result.Scope.Should().Be(SearchScope.Subtree);
            
            result.DistinguishedName.Should().Be("nm");
        }
    }
}
