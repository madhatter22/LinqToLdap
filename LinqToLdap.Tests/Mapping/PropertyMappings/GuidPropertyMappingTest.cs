using System;
using System.DirectoryServices.Protocols;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping.PropertyMappings;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class GuidPropertyMappingTest
    {
        private PropertyMappingArguments<GuidPropertyMappingTest> _mappingArguments;
        private Guid _guid;

        [TestInitialize]
        public void SetUp()
        {
            _guid = new Guid(new byte[]{1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16});

            _mappingArguments = new PropertyMappingArguments<GuidPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_Guid_ReturnsStringOctet()
        {
            //prepare
            var propertyMapping = new GuidPropertyMapping<GuidPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueToFilter(_guid);

            //assert
            value.Should().Be(_guid.ToStringOctet());
        }

        [TestMethod]
        public void FormatValueFromDirectory_ByteArray_ReturnsGuid()
        {
            //prepare
            var propertyMapping = new GuidPropertyMapping<GuidPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", _guid.ToByteArray()), "dn");

            //assert
            value.CastTo<Guid>().ToByteArray().Should().ContainInOrder(_guid.ToByteArray());
        }
        
        [TestMethod]
        public void FormatValueFromDirectory_NullAndNullable_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Guid?);
            var propertyMapping = new GuidPropertyMapping<GuidPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().BeNull();
        }

        [TestMethod]
        public void FormatValueFromDirectory_NullAndNotNullable_ThrowsMappingException()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Guid);
            var propertyMapping = new GuidPropertyMapping<GuidPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => propertyMapping.FormatValueFromDirectory(null, "dn"))
                .Should().Throw<MappingException>();
        }

        [TestMethod]
        public void IsEqual_DiffferentGuids_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => Guid.NewGuid();
            var propertyMapping = new GuidPropertyMapping<GuidPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, _guid, out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new GuidPropertyMapping<GuidPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, _guid, out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_SameGuids_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => new Guid(_guid.ToString());
            var propertyMapping = new GuidPropertyMapping<GuidPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, _guid, out modification);

            //assert
            value.Should().BeTrue();
            modification.Should().BeNull();
        }
    }
}
