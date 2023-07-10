using System.DirectoryServices.Protocols;
using LinqToLdap.Mapping.PropertyMappings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    public class StringPropertyMappingTest
    {
        private PropertyMappingArguments<StringPropertyMappingTest> _mappingArguments;

        [TestInitialize]
        public void SetUp()
        {
            _mappingArguments = new PropertyMappingArguments<StringPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_String_ReturnsString()
        {
            //prepare
            var mapping = new StringPropertyMapping<StringPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueToFilter("str");

            //assert
            value.Should().Be("str");
        }

        [TestMethod]
        public void FormatValueToFilter_StringBadCharacters_ReturnsFilteredString()
        {
            //prepare
            var mapping = new StringPropertyMapping<StringPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueToFilter("a\\*()\u0000z");

            //assert
            value.Should().Be("a\\5c\\2a\\28\\29\\00z");
        }

        [TestMethod]
        public void FormatValueFromDirectory_String_ReturnsString()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(string);
            var mapping = new StringPropertyMapping<StringPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueFromDirectory(new DirectoryAttribute("name", "str"), "dn");

            //assert
            value.Should().Be("str");
        }

        [TestMethod]
        public void FormatValueFromDirectory_ZeroLength_ReturnsEmptyString()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(string);
            var mapping = new StringPropertyMapping<StringPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueFromDirectory(new DirectoryAttribute("name"), "dn");

            //assert
            value.Should().Be(string.Empty);
        }

        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(string);
            var mapping = new StringPropertyMapping<StringPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().BeNull();
        }

        [TestMethod]
        public void GetDirectoryAttributeModification_String_ReturnsString()
        {
            //prepare
            _mappingArguments.Getter = t => "str";
            var propertyMapping = new StringPropertyMapping<StringPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.GetDirectoryAttributeModification(this);

            //assert
            value.Name.Should().Be(_mappingArguments.AttributeName);
            value.Operation.Should().Be(DirectoryAttributeOperation.Replace);
            value[0].Should().Be("str");
        }

        [TestMethod]
        public void IsEqual_DiffferentValues_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => "2";
            var propertyMapping = new StringPropertyMapping<StringPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;

            //act
            var value = propertyMapping.IsEqual(this, "1", out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new StringPropertyMapping<StringPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, "2", out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_SameValues_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => "4";
            var propertyMapping = new StringPropertyMapping<StringPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;

            //act
            var value = propertyMapping.IsEqual(this, "4", out modification);

            //assert
            value.Should().BeTrue();
            modification.Should().BeNull();
        }
    }
}
