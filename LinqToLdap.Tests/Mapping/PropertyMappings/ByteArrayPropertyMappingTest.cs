using System.DirectoryServices.Protocols;
using LinqToLdap.Mapping.PropertyMappings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class ByteArrayPropertMappingTest
    {
        private PropertyMappingArguments<ByteArrayPropertMappingTest> _mappingArguments;

        [TestInitialize]
        public void SetUp()
        {
            _mappingArguments = new PropertyMappingArguments<ByteArrayPropertMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_ByteArray_ReturnsStringOctet()
        {
            //prepare
            var bytes = new byte[] {1, 2, 3, 4};
            var propertyMapping = new ByteArrayPropertyMapping<ByteArrayPropertMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueToFilter(bytes);

            //assert
            value.Should().Be.EqualTo(bytes.ToStringOctet());
        }

        [TestMethod]
        public void FormatValueFromDirectory_ByteArray_ReturnsByteArray()
        {
            //prepare
            var bytes = new byte[] { 1, 2, 3, 4 };
            var propertyMapping = new ByteArrayPropertyMapping<ByteArrayPropertMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", bytes), "dn");

            //assert
            value.Should().Be.EqualTo(bytes);
        }

        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(byte[]);
            var propertyMapping = new ByteArrayPropertyMapping<ByteArrayPropertMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void IsEqual_SameLengthDifferentArrays_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new byte[] { 1, 2, 3 };
            var propertyMapping = new ByteArrayPropertyMapping<ByteArrayPropertMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new byte[] { 1, 2, 7 }, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_DifferentLengths_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new byte[] { 1, 2, 3 };
            var propertyMapping = new ByteArrayPropertyMapping<ByteArrayPropertMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new byte[] { 1, 2 }, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new ByteArrayPropertyMapping<ByteArrayPropertMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new byte[] { 1, 2, 3 }, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_SameArrays_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => new byte[] { 1, 2, 3 };
            var propertyMapping = new ByteArrayPropertyMapping<ByteArrayPropertMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new byte[] { 1, 2, 3 }, out modification);

            //assert
            value.Should().Be.True();
            modification.Should().Be.Null();
        }
    }
}
