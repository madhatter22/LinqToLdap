using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using LinqToLdap.Collections;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using LinqToLdap.Transformers;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Transformers
{
    [TestClass]
    public class ResultTransformerTest
    {
        private Mock<IObjectMapping> _mapping;
#if (!NET35 && !NET40)
        private System.Collections.ObjectModel.ReadOnlyDictionary<string, string> _properties;
#else
        private LinqToLdap.Collections.ReadOnlyDictionary<string, string> _properties;
#endif

        private Mock<IPropertyMapping> _property1;
        private Mock<IPropertyMapping> _property2;
        private Mock<IPropertyMapping> _property3;
        private Mock<IPropertyMapping> _property4;
        private ResultTransformer _transformer;
        private readonly byte[] _bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
        private readonly string[] _strings = new[] { "one", "two", "three", "four" };

        [TestInitialize]
        public void SetUp()
        {
            SetUpPropertyMappings();
            _mapping = new Mock<IObjectMapping>();
            _mapping.Setup(m => m.Properties)
                .Returns(_properties);

            _mapping.Setup(m => m.GetPropertyMappings())
                .Returns(new List<IPropertyMapping>
                             {
                                 _property1.Object,
                                 _property2.Object,
                                 _property3.Object,
                                 _property4.Object
                             });

            _mapping.Setup(m => m.GetPropertyMapping("Property1", typeof(object)))
                .Returns(_property1.Object);

            _mapping.Setup(m => m.GetPropertyMapping("Property2", typeof(object)))
                .Returns(_property2.Object);

            _mapping.Setup(m => m.GetPropertyMapping("Property3", typeof(object)))
                .Returns(_property3.Object);

            _mapping.Setup(m => m.GetPropertyMapping("Property4", typeof(object)))
                .Returns(_property4.Object);
        }

        private void SetUpPropertyMappings()
        {
            _property1 = new Mock<IPropertyMapping>();
            _property1.Setup(p => p.AttributeName)
                .Returns("Property1");
            _property1.Setup(p => p.FormatValueFromDirectory(It.IsAny<DirectoryAttribute>(), "dn"))
                .Returns("prop1");
            _property1.Setup(p => p.Default())
                .Returns(default(string));

            _property2 = new Mock<IPropertyMapping>();
            _property2.Setup(p => p.AttributeName)
                .Returns("Property2");
            _property2.Setup(p => p.FormatValueFromDirectory(It.IsAny<DirectoryAttribute>(), "dn"))
                .Returns("2");
            _property2.Setup(p => p.Default())
                .Returns(default(int));

            _property3 = new Mock<IPropertyMapping>();
            _property3.Setup(p => p.AttributeName)
                .Returns("Property3");
            _property3.Setup(p => p.FormatValueFromDirectory(It.IsAny<DirectoryAttribute>(), "dn"))
                .Returns(_bytes);
            _property3.Setup(p => p.Default())
                .Returns(default(byte[]));

            _property4 = new Mock<IPropertyMapping>();
            _property4.Setup(p => p.AttributeName)
                .Returns("Property4");
            _property4.Setup(p => p.FormatValueFromDirectory(It.IsAny<DirectoryAttribute>(), "dn"))
                .Returns(_strings);
            _property4.Setup(p => p.Default())
                .Returns(default(string[]));
        }

        [TestMethod]
        public void Transform_NonAnonymousTypeAllPropertiesPresent_SetsAllPropertyValuesAndReturnsInstance()
        {
            //prepare
            _properties = new Dictionary<string, string>
                                        {
                                            {"Property1", "Property1"},
                                            {"Property2", "Property2"},
                                            {"Property3", "Property3"},
                                            {"Property4", "Property4"}
                                        }.ToReadOnlyDictionary();
            _transformer = new ResultTransformer(_properties, _mapping.Object);

            var instance = new object();
            _mapping.Setup(m => m.Create(It.IsAny<object[]>(), null))
                .Returns(instance);
            _mapping.Setup(m => m.IsForAnonymousType)
                .Returns(false);

            var searchResultAttributesCollection =
                typeof(SearchResultAttributeCollection).Create<SearchResultAttributeCollection>();
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property1", new DirectoryAttribute("Property1", "prop1") });
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property2", new DirectoryAttribute("Property2", "2") });
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property3", new DirectoryAttribute("Property3", _bytes) });
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property4", new DirectoryAttribute("Property4", _strings) });

            var searchResultsEntry =
                typeof(SearchResultEntry).Create<SearchResultEntry>(
                    new object[] { "dn", searchResultAttributesCollection });

            //act
            var transformed = _transformer.Transform(searchResultsEntry);

            //assert
            transformed.Should().Be.SameInstanceAs(instance);
            _property1.Verify(p => p.SetValue(instance, "prop1"));
            _property2.Verify(p => p.SetValue(instance, "2"));
            _property3.Verify(p => p.SetValue(instance, _bytes));
            _property4.Verify(p => p.SetValue(instance, _strings));
        }

        [TestMethod]
        public void Transform_NonAnonymousTypeSomePropertiesPresent_SetsPresentPropertyValuesAndReturnsInstance()
        {
            //prepare
            _properties = new Dictionary<string, string>
                                        {
                                            {"Property1", "Property1"},
                                            {"Property2", "Property2"}
                                        }.ToReadOnlyDictionary();
            _transformer = new ResultTransformer(_properties, _mapping.Object);

            var instance = new object();
            _mapping.Setup(m => m.Create(It.IsAny<object[]>(), null))
                .Returns(instance);
            _mapping.Setup(m => m.IsForAnonymousType)
                .Returns(false);

            var searchResultAttributesCollection =
                typeof(SearchResultAttributeCollection).Create<SearchResultAttributeCollection>();
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property1", new DirectoryAttribute("Property1", "prop1") });
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property2", new DirectoryAttribute("Property2", "2") });

            var searchResultsEntry =
                typeof(SearchResultEntry).Create<SearchResultEntry>(
                    new object[] { "dn", searchResultAttributesCollection });

            //act
            var transformed = _transformer.Transform(searchResultsEntry);

            //assert
            transformed.Should().Be.SameInstanceAs(instance);
            _property1.Verify(p => p.SetValue(instance, "prop1"));
            _property2.Verify(p => p.SetValue(instance, "2"));
            _property3.Verify(p => p.SetValue(instance, _bytes), Times.Never());
            _property4.Verify(p => p.SetValue(instance, _strings), Times.Never());
        }

        [TestMethod]
        public void Transform_non_anonymous_type_with_catch_all_mapping_sets_present_properties_and_sets_catch_all_and_returns_instance()
        {
            //prepare
            _properties = new Dictionary<string, string>
                                        {
                                            {"Property1", "Property1"},
                                            {"Property2", "Property2"}
                                        }.ToReadOnlyDictionary();

            _mapping.Setup(m => m.GetPropertyMappingByAttribute("property1", typeof(object)))
                .Returns(_property1.Object);

            _mapping.Setup(m => m.GetPropertyMappingByAttribute("property2", typeof(object)))
                .Returns(_property2.Object);

            _mapping.Setup(m => m.GetPropertyMappingByAttribute("property3", typeof(object)))
                .Returns(_property3.Object);

            _mapping.Setup(m => m.GetPropertyMappingByAttribute("property4", typeof(object)))
                .Returns(_property4.Object);

            _transformer = new ResultTransformer(_properties, _mapping.Object);

            var instance = new object();
            _mapping.Setup(m => m.Create(It.IsAny<object[]>(), null))
                .Returns(instance);
            _mapping.Setup(m => m.IsForAnonymousType)
                .Returns(false);

            _mapping.Setup(x => x.HasCatchAllMapping)
                .Returns(true);

            var catchAll = new Mock<IPropertyMapping>();
            _mapping.Setup(x => x.GetCatchAllMapping())
                .Returns(catchAll.Object);

            var searchResultAttributesCollection =
                typeof(SearchResultAttributeCollection).Create<SearchResultAttributeCollection>();
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property1", new DirectoryAttribute("Property1", "prop1") });
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property2", new DirectoryAttribute("Property2", "2") });
            searchResultAttributesCollection
                .Call("Add", new object[] { "OtherProperty", new DirectoryAttribute("OtherProperty", "2") });

            var searchResultsEntry =
                typeof(SearchResultEntry).Create<SearchResultEntry>(
                    new object[] { "dn", searchResultAttributesCollection });

            //act
            var transformed = _transformer.Transform(searchResultsEntry);

            //assert
            transformed.Should().Be.SameInstanceAs(instance);
            _property1.Verify(p => p.SetValue(instance, "prop1"));
            _property2.Verify(p => p.SetValue(instance, "2"));
            _property3.Verify(p => p.SetValue(instance, _bytes), Times.Never());
            _property4.Verify(p => p.SetValue(instance, _strings), Times.Never());
            catchAll.Verify(
                p => p.SetValue(instance, It.Is<IDirectoryAttributes>(da => da.Entry == searchResultsEntry)), Times.Once());
        }

        [TestMethod]
        public void Transform_AnonymousTypeAllPropertiesPresent_SetsAllPropertyValuesAndReturnsInstance()
        {
            //prepare
            _properties = new Dictionary<string, string>
                                        {
                                            {"Property1", "Property1"},
                                            {"Property2", "Property2"},
                                            {"Property3", "Property3"},
                                            {"Property4", "Property4"}
                                        }.ToReadOnlyDictionary();
            _transformer = new ResultTransformer(_properties, _mapping.Object);

            var instance = new object();
            _mapping.Setup(m => m.Properties)
                .Returns(_properties);
            _mapping.Setup(m => m.Create(new object[] { "prop1", "2", _bytes, _strings }, null))
                .Returns(instance);
            _mapping.Setup(m => m.IsForAnonymousType)
                .Returns(true);

            var searchResultAttributesCollection =
                typeof(SearchResultAttributeCollection).Create<SearchResultAttributeCollection>();
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property1", new DirectoryAttribute("Property1", "prop1") });
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property2", new DirectoryAttribute("Property2", "2") });
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property3", new DirectoryAttribute("Property3", _bytes) });
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property4", new DirectoryAttribute("Property4", _strings) });

            var searchResultsEntry =
                typeof(SearchResultEntry).Create<SearchResultEntry>(
                    new object[] { "dn", searchResultAttributesCollection });

            //act
            var transformed = _transformer.Transform(searchResultsEntry);

            //assert
            transformed.Should().Be.SameInstanceAs(instance);
        }

        [TestMethod]
        public void Transform_AnonymousTypeSomePropertiesPresent_SetsPresentPropertyValuesAndReturnsInstance()
        {
            //prepare
            _properties = new Dictionary<string, string>
                                        {
                                            {"Property1", "Property1"},
                                            {"Property2", "Property2"},
                                            {"Property3", "Property3"},
                                            {"Property4", "Property4"}
                                        }.ToReadOnlyDictionary();
            _transformer = new ResultTransformer(_properties, _mapping.Object);

            var instance = new object();
            _mapping.Setup(m => m.Properties)
                .Returns(_properties);
            _mapping.Setup(m => m.Create(new object[] { "prop1", default(int), default(byte[]), default(string[]) }, null))
                .Returns(instance);
            _mapping.Setup(m => m.IsForAnonymousType)
                .Returns(true);

            var searchResultAttributesCollection =
                typeof(SearchResultAttributeCollection).Create<SearchResultAttributeCollection>();
            searchResultAttributesCollection
                .Call("Add", new object[] { "Property1", new DirectoryAttribute("Property1", "prop1") });

            var searchResultsEntry =
                typeof(SearchResultEntry).Create<SearchResultEntry>(
                    new object[] { "dn", searchResultAttributesCollection });

            //act
            var transformed = _transformer.Transform(searchResultsEntry);

            //assert
            transformed.Should().Be.SameInstanceAs(instance);
        }
    }
}