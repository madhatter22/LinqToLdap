using System;
using System.Collections.Generic;
using System.Linq;
using LinqToLdap.Collections;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.ClassMapAssembly;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;

namespace LinqToLdap.Tests.Mapping
{
    [TestClass]
    public class ObjectMappingTests
    {
        private Mock<IPropertyMapping> _distinguishedName;
        private Mock<IPropertyMapping> _storeGenerated;
        private Mock<IPropertyMapping> _readOnly;
        private Mock<IPropertyMapping> _updateable;

        [TestInitialize]
        public void Setup()
        {
            _distinguishedName = new Mock<IPropertyMapping>();
            _distinguishedName.Setup(x => x.AttributeName)
                .Returns("DistinguishedName");
            _distinguishedName.Setup(x => x.IsDistinguishedName)
                .Returns(true);
            _distinguishedName.Setup(x => x.PropertyName)
                .Returns("DistinguishedName");
            _storeGenerated = new Mock<IPropertyMapping>();
            _storeGenerated.Setup(x => x.ReadOnly)
                .Returns(ReadOnly.OnUpdate);
            _storeGenerated.Setup(x => x.AttributeName)
                .Returns("StoreGenerated");
            _storeGenerated.Setup(x => x.PropertyName)
                .Returns("IsStoreGenerated");
            _readOnly = new Mock<IPropertyMapping>();
            _readOnly.Setup(x => x.ReadOnly)
                .Returns(ReadOnly.Always);
            _readOnly.Setup(x => x.AttributeName)
                .Returns("ReadOnly");
            _readOnly.Setup(x => x.PropertyName)
                .Returns("IsReadOnly");
            _updateable = new Mock<IPropertyMapping>();
            _updateable.Setup(x => x.AttributeName)
                .Returns("Updateable");
            _updateable.Setup(x => x.PropertyName)
                .Returns("IsUpdateable");
        }

        [TestMethod]
        public void Constructor_Initializes_Correctly()
        {
            //arrange
            var propertyMappings = new[]
            {_distinguishedName.Object, _storeGenerated.Object, _readOnly.Object, _updateable.Object};

            //act
            var mapping = new TestObjectMapping("context", propertyMappings,
                "category", false, new[] { "class" }, false);

            //assert
            mapping.NamingContext.Should().Be("context");
            mapping.ObjectCategory.Should().Be("category");
            mapping.ObjectClasses.Should().ContainInOrder(new[] { "class" });
            propertyMappings.ToList().ForEach(x => mapping.GetPropertyMappings()
                .Should().Contain(x));
            mapping.GetDistinguishedNameMapping().Should().Be(_distinguishedName.Object);
            mapping.GetPropertyMappingsForAdd().Should().Contain(_updateable.Object).And.Contain(_storeGenerated.Object).And.HaveCount(2);
            mapping.GetPropertyMappingsForUpdate().Should().Contain(_updateable.Object).And.HaveCount(1);
            mapping.IncludeObjectCategory.Should().BeFalse();
            mapping.IncludeObjectClasses.Should().BeFalse();
            mapping.Properties.ForEach(x =>
            {
                var result = (x.Key == _distinguishedName.Object.PropertyName &&
                              x.Value == _distinguishedName.Object.AttributeName) ||
                             (x.Key == _storeGenerated.Object.PropertyName &&
                              x.Value == _storeGenerated.Object.AttributeName) ||
                             (x.Key == _readOnly.Object.PropertyName &&
                              x.Value == _readOnly.Object.AttributeName) ||
                             (x.Key == _updateable.Object.PropertyName &&
                              x.Value == _updateable.Object.AttributeName);

                result.Should().BeTrue();
            });
            mapping.Properties.Count.Should().Be(4);
        }

        [TestMethod]
        public void AddSubTypeMapping_Should_Reinitialize_PropertyNames_And_Merge_SubType_PropertyNames()
        {
            //arrange
            var propertyMappings = new[]
            {_distinguishedName.Object, _storeGenerated.Object, _readOnly.Object, _updateable.Object};

            var mapping = new TestObjectMapping("context", propertyMappings,
                "category", false, new[] { "class" }, false);

            mapping.Properties.ForEach(x =>
            {
                var result = (x.Key == _distinguishedName.Object.PropertyName &&
                              x.Value == _distinguishedName.Object.AttributeName) ||
                             (x.Key == _storeGenerated.Object.PropertyName &&
                              x.Value == _storeGenerated.Object.AttributeName) ||
                             (x.Key == _readOnly.Object.PropertyName &&
                              x.Value == _readOnly.Object.AttributeName) ||
                             (x.Key == _updateable.Object.PropertyName &&
                              x.Value == _updateable.Object.AttributeName);

                result.Should().BeTrue();
            });
            mapping.Properties.Count.Should().Be(4);

            var subTypePropertyMapping = new Mock<IObjectMapping>();
            subTypePropertyMapping.Setup(x => x.ObjectClasses)
                .Returns(new[] { "sub" });
            subTypePropertyMapping.Setup(x => x.Type)
                .Returns(typeof(SubType));
            subTypePropertyMapping.Setup(x => x.Properties)
                .Returns(
                    new[]
                    {
                        new KeyValuePair<string, string>("IsSubType", "SubType"),
                        new KeyValuePair<string, string>(_distinguishedName.Object.PropertyName, _distinguishedName.Object.AttributeName)
                    }.ToDictionary(x => x.Key, x => x.Value).ToReadOnlyDictionary());

            //act
            mapping.AddSubTypeMapping(subTypePropertyMapping.Object);

            //assert
            mapping.Properties.ForEach(x =>
            {
                var result = (x.Key == _distinguishedName.Object.PropertyName &&
                              x.Value == _distinguishedName.Object.AttributeName) ||
                             (x.Key == _storeGenerated.Object.PropertyName &&
                              x.Value == _storeGenerated.Object.AttributeName) ||
                             (x.Key == _readOnly.Object.PropertyName &&
                              x.Value == _readOnly.Object.AttributeName) ||
                             (x.Key == _updateable.Object.PropertyName &&
                              x.Value == _updateable.Object.AttributeName) ||
                             (x.Key == "IsSubType" &&
                              x.Value == "SubType");

                result.Should().BeTrue();
            });
            mapping.Properties.Count.Should().Be(5);
        }

        [TestMethod]
        public void GetPropertyMapping_null_owning_type_returns_property_name_from_mapping_instance()
        {
            //arrange
            var propertyMappings = new[]
            {_distinguishedName.Object, _storeGenerated.Object, _readOnly.Object, _updateable.Object};

            var mapping = new TestObjectMapping("context", propertyMappings,
                "category", false, new[] { "class" }, false);

            var subTypePropertyMapping = new Mock<IObjectMapping>();
            subTypePropertyMapping.Setup(x => x.ObjectClasses)
                .Returns(new[] { "sub" });
            subTypePropertyMapping.Setup(x => x.Type)
                .Returns(typeof(SubType));
            subTypePropertyMapping.Setup(x => x.Properties)
                .Returns(
                    new[]
                    {
                        new KeyValuePair<string, string>("IsSubType", "SubType")
                    }.ToDictionary(x => x.Key, x => x.Value).ToReadOnlyDictionary());
            mapping.AddSubTypeMapping(subTypePropertyMapping.Object);

            //act
            var propertyMapping = mapping.GetPropertyMapping("DistinguishedName");

            //assert
            propertyMapping.Should().Be(_distinguishedName.Object);
        }

        [TestMethod]
        public void GetPropertyMapping_mapping_from_parent_type_returns_property_name_from_mapping_instance()
        {
            //arrange
            var propertyMappings = new[]
            {_distinguishedName.Object, _storeGenerated.Object, _readOnly.Object, _updateable.Object};

            var mapping = new TestObjectMapping("context", propertyMappings,
                "category", false, new[] { "class" }, false);
            mapping.Type.Should().Be(typeof(ParentType));

            var subTypePropertyMapping = new Mock<IObjectMapping>();
            subTypePropertyMapping.Setup(x => x.ObjectClasses)
                .Returns(new[] { "sub" });
            subTypePropertyMapping.Setup(x => x.Type)
                .Returns(typeof(SubType));
            subTypePropertyMapping.Setup(x => x.Properties)
                .Returns(
                    new[]
                    {
                        new KeyValuePair<string, string>("IsSubType", "SubType")
                    }.ToDictionary(x => x.Key, x => x.Value).ToReadOnlyDictionary());
            mapping.AddSubTypeMapping(subTypePropertyMapping.Object);

            //act
            var propertyMapping = mapping.GetPropertyMapping("DistinguishedName", mapping.Type);

            //assert
            propertyMapping.Should().Be(_distinguishedName.Object);
        }

        [TestMethod]
        public void GetPropertyMapping_mapping_from_sub_type_returns_property_name_from_mapping_instance()
        {
            //arrange
            var propertyMappings = new[]
            {_distinguishedName.Object, _storeGenerated.Object, _readOnly.Object, _updateable.Object};

            var mapping = new TestObjectMapping("context", propertyMappings,
                "category", false, new[] { "class" }, false);
            mapping.Type.Should().Be(typeof(ParentType));

            var subTypePropertyMapping = new Mock<IPropertyMapping>();
            var subTypeMapping = new Mock<IObjectMapping>();
            subTypeMapping.Setup(x => x.ObjectClasses)
                .Returns(new[] { "sub" });
            subTypeMapping.Setup(x => x.Type)
                .Returns(typeof(SubType));
            subTypeMapping.Setup(x => x.Properties)
                .Returns(
                    new[]
                    {
                        new KeyValuePair<string, string>("IsSubType", "SubType")
                    }.ToDictionary(x => x.Key, x => x.Value).ToReadOnlyDictionary());
            subTypeMapping.Setup(x => x.GetPropertyMapping("IsSubType", null))
                .Returns(subTypePropertyMapping.Object);
            mapping.AddSubTypeMapping(subTypeMapping.Object);

            //act
            var propertyMapping = mapping.GetPropertyMapping("IsSubType", typeof(SubType));

            //assert
            propertyMapping.Should().Be(subTypePropertyMapping.Object);
        }

        [TestMethod]
        public void GetPropertyMapping_unknown_mapping_should_throw_exception()
        {
            //arrange
            var propertyMappings = new[]
            {_distinguishedName.Object, _storeGenerated.Object, _readOnly.Object, _updateable.Object};

            var mapping = new TestObjectMapping("context", propertyMappings,
                "category", false, new[] { "class" }, false);
            mapping.Type.Should().Be(typeof(ParentType));

            //assert
            Executing.This(() => mapping.GetPropertyMapping("error"))
                .Should()
                .Throw<MappingException>()
                .And.Message.Should()
                .Be($"Property mapping with name 'error' was not found for '{mapping.Type.FullName}'");
        }

        [TestMethod]
        public void GetPropertyMapping_unknown_mapping_with_sub_type_mappings_should_return_null()
        {
            //arrange
            var propertyMappings = new[]
            {_distinguishedName.Object, _storeGenerated.Object, _readOnly.Object, _updateable.Object};

            var mapping = new TestObjectMapping("context", propertyMappings,
                "category", false, new[] { "class" }, false);
            mapping.Type.Should().Be(typeof(ParentType));
            var subTypePropertyMapping = new Mock<IPropertyMapping>();
            var subTypeMapping = new Mock<IObjectMapping>();
            subTypeMapping.Setup(x => x.ObjectClasses)
                .Returns(new[] { "sub" });
            subTypeMapping.Setup(x => x.Type)
                .Returns(typeof(SubType));
            subTypeMapping.Setup(x => x.Properties)
                .Returns(
                    new[]
                    {
                        new KeyValuePair<string, string>("IsSubType", "SubType")
                    }.ToDictionary(x => x.Key, x => x.Value).ToReadOnlyDictionary());
            subTypeMapping.Setup(x => x.GetPropertyMapping("IsSubType", null))
                .Returns(subTypePropertyMapping.Object);
            mapping.AddSubTypeMapping(subTypeMapping.Object);

            //assert
            mapping.GetPropertyMapping("error").Should().BeNull();
        }

        private class TestObjectMapping : ObjectMapping
        {
            public TestObjectMapping(string namingContext, IEnumerable<IPropertyMapping> propertyMappings, string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClass = null, bool includeObjectClasses = true) : base(namingContext, propertyMappings, objectCategory, includeObjectCategory, objectClass, includeObjectClasses)
            {
                IsForAnonymousType = false;
                Type = typeof(ParentType);
            }

            public override Type Type { get; }
            public override bool IsForAnonymousType { get; }

            public override object Create(object[] parameters = null, object[] objectClasses = null)
            {
                throw new NotImplementedException();
            }
        }

        private class ParentType
        {
        }

        private class SubType : ParentType
        {
        }
    }
}