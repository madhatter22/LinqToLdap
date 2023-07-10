using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Xml;
using LinqToLdap.Collections;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;
using LinqToLdap.Tests.TestSupport;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using LinqToLdap.Tests.TestSupport.QueryCommands;
using LinqToLdap.Transformers;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System;

namespace LinqToLdap.Tests.QueryCommands
{
    [TestClass]
    public class StandardQueryCommandTest
    {
        private Mock<IObjectMapping> _mapping;
        private Mock<IQueryCommandOptions> _options;

        [TestInitialize]
        public void SetUp()
        {
            _options = new Mock<IQueryCommandOptions>();
            _options.Setup(o => o.AttributesToLoad)
                .Returns(new Dictionary<string, string> { { "cn", "cn" } });
            _mapping = new Mock<IObjectMapping>();
            _mapping.Setup(m => m.NamingContext)
                .Returns("nm");
        }

        [TestMethod]
        public void Execute_WithSortOptionsAndNoPagingOptions_CallsHandleStandardRequest()
        {
            //prepare
            var sort = new SortingOptions();
            sort.AddSort("test", true);
            _options.Setup(o => o.SortingOptions)
                .Returns(sort);
            var command = new MockStandardQueryCommand(_options.Object, _mapping.Object)
                .DisablePagedRequest()
                .DisableStandardRequest();

            //act
            command.Execute(null, SearchScope.OneLevel, 1, true);

            //assert
            command.GetRequest().Controls.OfType<SortRequestControl>().Should().HaveCount(1);
            command.HandleStandardRequestCalled.Should().BeTrue();
            command.HandlePagedRequestCalled.Should().BeFalse();
        }

        [TestMethod]
        public void Constructor_object_mapping_has_catch_all_mapping_does_not_initialize_attrbiutes()
        {
            //prepare
            _mapping.Setup(x => x.HasCatchAllMapping)
                .Returns(true);

            //act
            var command = new MockStandardQueryCommand(_options.Object, _mapping.Object);

            //assert
            command.GetRequest().Attributes.Count.Should().Be(0);
        }

        [TestMethod]
        public void Constructor_initialize_attrbiutes()
        {
            //act
            var command = new MockStandardQueryCommand(_options.Object, _mapping.Object);

            //assert
            command.GetRequest().Attributes.Count.Should().Be(1);
            command.GetRequest().Attributes.Cast<string>().Should().Contain("cn");
        }

        [TestMethod]
        public void Execute_WithSortOptionsAndAutoPagingOptions_CallsHandlePagedRequest()
        {
            //prepar
            var sort = new SortingOptions();
            sort.AddSort("test", true);
            _options.Setup(o => o.SortingOptions)
                .Returns(sort);
            _options.Setup(o => o.PagingOptions)
                .Returns(new PagingOptions(1, null));
            var command = new MockStandardQueryCommand(_options.Object, _mapping.Object)
                .DisablePagedRequest()
                .DisableStandardRequest();

            //act
            command.Execute(null, SearchScope.OneLevel, 1, true);

            //assert
            command.GetRequest().Controls.OfType<SortRequestControl>().Should().HaveCount(1);
            command.HandleStandardRequestCalled.Should().BeFalse();
            command.HandlePagedRequestCalled.Should().BeTrue();
        }

        [TestMethod]
        public void Execute_WithSortOptionsAndManualPagingOptions_CallsHandlePagedRequest()
        {
            //prepare
            var sort = new SortingOptions();
            sort.AddSort("test", true);
            _options.Setup(o => o.SortingOptions)
                .Returns(sort);
            _options.Setup(o => o.Controls)
                .Returns(new[] { new PageResultRequestControl() });
            var command = new MockStandardQueryCommand(_options.Object, _mapping.Object)
                .DisablePagedRequest()
                .DisableStandardRequest();

            //act
            command.Execute(null, SearchScope.OneLevel, 1, true);

            //assert
            command.GetRequest().Controls.OfType<SortRequestControl>().Should().HaveCount(1);
            command.HandleStandardRequestCalled.Should().BeFalse();
            command.HandlePagedRequestCalled.Should().BeTrue();
        }

        [TestMethod]
        public void Execute_WithSortOptionsAndManualSortOptions_ThrowsInvalidOperationException()
        {
            //prepare
            var sort = new SortingOptions();
            sort.AddSort("test", true);
            _options.Setup(o => o.SortingOptions)
                .Returns(sort);
            _options.Setup(o => o.Controls)
                .Returns(new[] { new SortRequestControl() });
            var command = new MockStandardQueryCommand(_options.Object, _mapping.Object)
                .DisablePagedRequest()
                .DisableStandardRequest();

            //assert
            Executing.This(() => command.Execute(null, SearchScope.OneLevel, 1, true))
                .Should().Throw<InvalidOperationException>()
                .And.GetBaseException().Message.Should().Be("Only one sort request control can be sent to the server");
        }

        [TestMethod]
        public void Execute_WithPagingOptionsAndManualPagingOptions_ThrowsInvalidOperationException()
        {
            //prepare
            _options.Setup(o => o.PagingOptions)
                .Returns(new PagingOptions(1, null));
            _options.Setup(o => o.Controls)
                .Returns(new[] { new PageResultRequestControl() });
            var command = new MockStandardQueryCommand(_options.Object, _mapping.Object)
                .DisablePagedRequest()
                .DisableStandardRequest();

            var func = () => command.Execute(null, SearchScope.OneLevel, 1, true);

            //assert
            Executing.This(() => command.Execute(null, SearchScope.OneLevel, 1, true))
                .Should().Throw<InvalidOperationException>()
                .And.GetBaseException().Message.Should().Be("Only one page request control can be sent to the server.");
        }

        [TestMethod]
        public void HandlePagedRequest_NullPagedRequest_CreatesPagedRequestFromOptions()
        {
            //prepare
            _options.Setup(o => o.Filter)
                .Returns("filter");
            _options.Setup(o => o.PagingOptions)
                .Returns(new PagingOptions(1, null));
            _options.Setup(o => o.GetEnumerator(It.IsAny<SearchResultEntryCollection>()))
                .Returns(new SearchResponseEnumerable<object>(new Mock<IResultTransformer>().Object,
                                                              new List<SearchResultEntry>()));
            _options.Setup(o => o.GetEnumeratorReturnType())
                .Returns(typeof(object));
            var xmlNode = new XmlDocument();
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
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(SearchRequest), response } });
            var command = new MockStandardQueryCommand(_options.Object, _mapping.Object)
                .DisableStandardRequest();
            command.ResponseControlsToSearch = new DirectoryControl[]
                                                   {
                                                       DirectoryControlFactory.CreatePageResponse(new byte[]{1,2}, new byte[0])
                                                   };

            //act
            var obj = command.HandlePagedRequest(connection, null, null);

            //assert
            command.GetRequest().Controls.OfType<PageResultRequestControl>().Should().HaveCount(1);
            obj.Should().BeOfType<LdapPage<object>>();
            obj.CastTo<LdapPage<object>>().PageSize.Should().Be(1);
            obj.CastTo<LdapPage<object>>().NextPage.Should().ContainInOrder(new byte[] { 1, 2 });
            obj.CastTo<LdapPage<object>>().Filter.Should().Be("filter");
        }

        [TestMethod]
        public void HandlePagedRequest_ExistingPagedRequest_IgnoresPagedRequestFromOptions()
        {
            //prepare
            _options.Setup(o => o.Filter)
                .Returns("filter");
            _options.Setup(o => o.GetEnumerator(It.IsAny<SearchResultEntryCollection>()))
                .Returns(new SearchResponseEnumerable<object>(new Mock<IResultTransformer>().Object,
                                                              new List<SearchResultEntry>()));
            _options.Setup(o => o.GetEnumeratorReturnType())
                .Returns(typeof(object));
            var xmlNode = new XmlDocument();
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
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(SearchRequest), response } });
            var command = new MockStandardQueryCommand(_options.Object, _mapping.Object)
                .DisableStandardRequest();
            command.ResponseControlsToSearch = new DirectoryControl[]
                                                   {
                                                       DirectoryControlFactory.CreatePageResponse(new byte[]{1,2}, new byte[0])
                                                   };

            //act
            var obj = command.HandlePagedRequest(connection, new PageResultRequestControl(2), null);

            //assert
            command.GetRequest().Controls.OfType<PageResultRequestControl>()
                .Should().HaveCount(0);
            obj.Should().BeOfType<LdapPage<object>>();
            obj.CastTo<LdapPage<object>>().PageSize.Should().Be(2);
            obj.CastTo<LdapPage<object>>().NextPage.Should().ContainInOrder(new byte[] { 1, 2 });
            obj.CastTo<LdapPage<object>>().Filter.Should().Be("filter");
        }
    }
}