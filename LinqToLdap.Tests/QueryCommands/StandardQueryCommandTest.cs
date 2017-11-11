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
using SharpTestsEx;
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
            command.GetRequest().Controls.OfType<SortRequestControl>().Should().Have.Count.EqualTo(1);
            command.HandleStandardRequestCalled.Should().Be.True();
            command.HandlePagedRequestCalled.Should().Be.False();
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
            command.GetRequest().Attributes.Count.Should().Be.EqualTo(0);
        }

        [TestMethod]
        public void Constructor_initialize_attrbiutes()
        {
            //act
            var command = new MockStandardQueryCommand(_options.Object, _mapping.Object);

            //assert
            command.GetRequest().Attributes.Count.Should().Be.EqualTo(1);
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
            command.GetRequest().Controls.OfType<SortRequestControl>().Should().Have.Count.EqualTo(1);
            command.HandleStandardRequestCalled.Should().Be.False();
            command.HandlePagedRequestCalled.Should().Be.True();
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
            command.GetRequest().Controls.OfType<SortRequestControl>().Should().Have.Count.EqualTo(1);
            command.HandleStandardRequestCalled.Should().Be.False();
            command.HandlePagedRequestCalled.Should().Be.True();
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
                .Returns(new[] {new SortRequestControl()});
            var command = new MockStandardQueryCommand(_options.Object, _mapping.Object)
                .DisablePagedRequest()
                .DisableStandardRequest();

            //assert
            Executing.This(() => command.Execute(null, SearchScope.OneLevel, 1, true))
                .Should().Throw<InvalidOperationException>()
                .And.Exception.Message.Should().Be.EqualTo("Only one sort request control can be sent to the server");
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

            //assert
            Executing.This(() => command.Execute(null, SearchScope.OneLevel, 1, true))
                .Should().Throw<InvalidOperationException>()
                .Exception.Message.Should().Be.EqualTo("Only one page request control can be sent to the server.");
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
                .Returns(typeof (object));
            var xmlNode = new XmlDocument();
            var response = typeof(SearchResponse).Create<SearchResponse>(xmlNode);
            response.SetFieldValue("result", ResultCode.Success);
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
            command.GetRequest().Controls.OfType<PageResultRequestControl>().Should().Have.Count.EqualTo(1);
            obj.Should().Be.InstanceOf<LdapPage<object>>();
            obj.As<LdapPage<object>>().PageSize.Should().Be.EqualTo(1);
            obj.As<LdapPage<object>>().NextPage.Should().Have.SameSequenceAs(new byte[]{1,2});
            obj.As<LdapPage<object>>().Filter.Should().Be.EqualTo("filter");
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
            var response = typeof(SearchResponse).Create<SearchResponse>(xmlNode);
            response.SetFieldValue("result", ResultCode.Success);
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
                .Should().Have.Count.EqualTo(0);
            obj.Should().Be.InstanceOf<LdapPage<object>>();
            obj.As<LdapPage<object>>().PageSize.Should().Be.EqualTo(2);
            obj.As<LdapPage<object>>().NextPage.Should().Have.SameSequenceAs(new byte[] { 1, 2 });
            obj.As<LdapPage<object>>().Filter.Should().Be.EqualTo("filter");
        }
    }
}
