using System.DirectoryServices.Protocols;
using LinqToLdap.Collections;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using LinqToLdap.Transformers;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Collections
{
    [TestClass]
    public class SearchResponseEnumeratorIntegrationTest
    {
        private Mock<IResultTransformer> _resultTransformer;

        [TestInitialize]
        public void SetUp()
        {
            _resultTransformer = new Mock<IResultTransformer>();
        }

        [TestMethod]
        public void Current_MockTransformer_CallsTransformOnMockTransformer()
        {
            //prepare
            var fake = new object();

            var searchResultAttributesCollection =
                typeof(SearchResultAttributeCollection).Create<SearchResultAttributeCollection>();

            var searchResultsEntry =
                typeof(SearchResultEntry).Create<SearchResultEntry>(
                    new object[] { "dn", searchResultAttributesCollection });

            var searchResultEntryCollection =
                typeof(SearchResultEntryCollection).Create<SearchResultEntryCollection>();

            searchResultEntryCollection
                .Call("Add", new object[] { searchResultsEntry });

            _resultTransformer.Setup(t => t.Transform(searchResultsEntry))
                .Returns(fake);

            
            //act
            var enumerator =
                new SearchResponseEnumerable<SearchResponseEnumeratorIntegrationTest>(searchResultEntryCollection,
                                                                                      _resultTransformer.Object).GetEnumerator();
            
            //assert
            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Should().BeSameAs(fake);
        }
    }
}
