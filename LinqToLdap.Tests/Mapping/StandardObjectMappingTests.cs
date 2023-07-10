using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinqToLdap.Collections;
using LinqToLdap.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;

namespace LinqToLdap.Tests.Mapping
{
    [TestClass]
    public class StandardObjectMappingTests
    {
        private Mock<IPropertyMapping> _distinguishedName;
        private StandardObjectMapping<ParentType> _mapping;
        private Mock<IObjectMapping> _subTypeMapping;
        private Mock<IObjectMapping> _subType2Mapping;

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

            var propertyMappings = new[] { _distinguishedName.Object };
            _subTypeMapping = new Mock<IObjectMapping>();
            _subTypeMapping.Setup(x => x.ObjectClasses)
                .Returns(new[] { "class", "sub" });
            _subTypeMapping.Setup(x => x.Type)
                .Returns(typeof(SubType));
            _subTypeMapping.Setup(x => x.Properties)
                .Returns(
                    new[]
                    {
                        new KeyValuePair<string, string>("IsSubType", "SubType")
                    }.ToDictionary(x => x.Key, x => x.Value).ToReadOnlyDictionary());
            _subType2Mapping = new Mock<IObjectMapping>();
            _subType2Mapping.Setup(x => x.ObjectClasses)
                .Returns(new[] { "class", "sub", "sub2" });
            _subType2Mapping.Setup(x => x.Type)
                .Returns(typeof(SubType2));
            _subType2Mapping.Setup(x => x.Properties)
                .Returns(
                    new[]
                    {
                        new KeyValuePair<string, string>("IsSubType", "SubType")
                    }.ToDictionary(x => x.Key, x => x.Value).ToReadOnlyDictionary());
            _mapping = new StandardObjectMapping<ParentType>("context", propertyMappings,
                "category", false, new[] { "class" }, false);
            _mapping.AddSubTypeMapping(_subTypeMapping.Object);
            _mapping.AddSubTypeMapping(_subType2Mapping.Object);
            _mapping.HasSubTypeMappings.Should().BeTrue();
        }

        [TestMethod]
        public void Create_null_objectclasses_returns_default_instance()
        {
            //arrange

            //act
            var instance = _mapping.Create();

            //assert
            instance.Should().BeOfType<ParentType>();
        }

        [TestMethod]
        public void Create_unknown_objectclasses_returns_default_instance()
        {
            //act
            var instance = _mapping.Create(null, new object[] {"nope"});

            //assert
            instance.Should().BeOfType<ParentType>();
        }

        [TestMethod]
        public void Create_objectclasses_that_match_subtype_returns_subtype_instance()
        {
            //arrange
            var objectClasses = new object[] {"class", "somethingelse", "sub"};
            _subTypeMapping.Setup(x => x.Create(null, null))
                .Returns(new SubType());

            //act
            var instance = _mapping.Create(null, objectClasses);

            //assert
            instance.Should().BeOfType<SubType>();
        }

        [TestMethod]
        public void Create_objectclasses_that_match_second_subtype_returns_second_subtype_instance()
        {
            //arrange
            var objectClasses = new object[] { "class", "somethingelse", "sub", "sub2" };
            _subType2Mapping.Setup(x => x.Create(null, null))
                .Returns(new SubType2());

            //act
            var instance = _mapping.Create(null, objectClasses);

            //assert
            instance.Should().BeOfType<SubType2>();
        }

        private class ParentType
        {
            
        }

        private class SubType : ParentType
        {
            
        }

        private class SubType2 : SubType
        {

        }
    }
}
