using System;
using System.DirectoryServices.Protocols;
using LinqToLdap.Mapping.PropertyMappings;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class StringArrayPropertyMappingTest
    {
        private PropertyMappingArguments<StringArrayPropertyMappingTest> _mappingArguments;

        [TestInitialize]
        public void SetUp()
        {
            _mappingArguments = new PropertyMappingArguments<StringArrayPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_StringArray_ThrowsNotSupportedException()
        {
            //prepare
            var strings = new[] { "one", "two", "three", "four" };
            var propertyMapping = new StringArrayPropertyMapping<StringArrayPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => propertyMapping.FormatValueToFilter(strings))
                .Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void FormatValueToFilter_String_ReturnsString()
        {
            //prepare
            var mapping = new StringPropertyMapping<StringArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueToFilter("str");

            //assert
            value.Should().Be.EqualTo("str");
        }

        [TestMethod]
        public void FormatValueFromDirectory_StringArray_ReturnsStringsArray()
        {
            //prepare
            var strings = new[] { "one", "two", "three", "four" };
            var propertyMapping = new StringArrayPropertyMapping<StringArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("names", strings), "dn");

            //assert
            value.As<string[]>().Should().Have.SameSequenceAs(strings);
        }

        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(string[]);
            var propertyMapping = new StringArrayPropertyMapping<StringArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void FormatValueFromDirectory_SingleString_ReturnsAsStringArray()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(string[]);
            var propertyMapping = new StringArrayPropertyMapping<StringArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", "str"), "dn");

            //assert
            value.As<string[]>().Should().Contain("str");
        }

        [TestMethod]
        public void IsEqual_SameLengthDifferentArrays_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new string[] { "1", "2", "3" };
            var propertyMapping = new StringArrayPropertyMapping<StringArrayPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new string[] { "1", "2", "7" }, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_DifferentLengths_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new string[] { "1", "2", "3" };
            var propertyMapping = new StringArrayPropertyMapping<StringArrayPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new string[] { "1", "2" }, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new StringArrayPropertyMapping<StringArrayPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new string[] { "1", "2", "3" }, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_SameArrays_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => new string[] { "1", "2", "3" };
            var propertyMapping = new StringArrayPropertyMapping<StringArrayPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new string[] { "1", "2", "3" }, out modification);

            //assert
            value.Should().Be.True();
            modification.Should().Be.Null();
        }
    }
}
