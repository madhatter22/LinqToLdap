using System;
using System.Collections.ObjectModel;
using System.DirectoryServices.Protocols;
using LinqToLdap.Mapping.PropertyMappings;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Collections.Generic;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class ByteArrayCollectionPropertyMappingTest
    {
        private PropertyMappingArguments<ByteArrayCollectionPropertyMappingTest> _mappingArguments;

        [TestInitialize]
        public void SetUp()
        {
            _mappingArguments = new PropertyMappingArguments<ByteArrayCollectionPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_ByteArrayArray_ThrowsNotSupportedException()
        {
            //prepare
            var bytes = new[] { new byte[] { 1, 2 }, new byte[] { 3, 4 } };
            var propertyMapping = new ByteArrayCollectionPropertyMapping<ByteArrayCollectionPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => propertyMapping.FormatValueToFilter(bytes))
                .Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void FormatValueToFilter_ByteArray_ReturnsStringOctet()
        {
            //prepare
            var bytes = new byte[] { 1, 2, 3, 4 };
            var mapping = new ByteArrayCollectionPropertyMapping<ByteArrayCollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueToFilter(bytes);

            //assert
            value.Should().Be(bytes.ToStringOctet());
        }

        [TestMethod]
        public void FormatValueFromDirectory_ByteArrayArray_ReturnsByteArrayCollection()
        {
            //prepare
            var bytes = new[] { new byte[] { 1, 2 }, new byte[] { 3, 4 } };
            var propertyMapping = new ByteArrayCollectionPropertyMapping<ByteArrayCollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("names", bytes), "dn");

            //assert
            value.CastTo<Collection<byte[]>>().Should().ContainInOrder(bytes);
        }

        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<byte[]>);
            var propertyMapping = new ByteArrayCollectionPropertyMapping<ByteArrayCollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().BeNull();
        }

        [TestMethod]
        public void FormatValueFromDirectory_SingleByteArray_ReturnsAsByteArrayCollection()
        {
            //prepare
            var bytes = new byte[] { 1, 2, 3, 4 };
            _mappingArguments.PropertyType = typeof(Collection<byte[]>);
            var propertyMapping = new ByteArrayCollectionPropertyMapping<ByteArrayCollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", bytes), "dn");

            //assert
            value.CastTo<Collection<byte[]>>().Should().Contain(bytes);
        }

        [TestMethod]
        public void IsEqual_SameLengthDifferentCollections_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new Collection<byte[]>(new List<byte[]> { new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 } });
            var propertyMapping = new ByteArrayCollectionPropertyMapping<ByteArrayCollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<byte[]>(new List<byte[]> { new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 7 } }), out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_DifferentLengths_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new Collection<byte[]>(new List<byte[]> { new byte[] { 1, 2, 3 } });
            var propertyMapping = new ByteArrayCollectionPropertyMapping<ByteArrayCollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<byte[]>(new List<byte[]> { new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 } }), out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new ByteArrayCollectionPropertyMapping<ByteArrayCollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new[] { new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 7 } }, out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_SameArrays_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new Collection<byte[]>(new List<byte[]> { new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 } });
            var propertyMapping = new ByteArrayCollectionPropertyMapping<ByteArrayCollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<byte[]>(new List<byte[]> { new byte[] { 1, 2, 3 }, new byte[] { 4, 5, 6 } }), out modification);

            //assert
            value.Should().BeTrue();
            modification.Should().BeNull();
        }
    }
}
