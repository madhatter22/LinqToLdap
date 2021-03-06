﻿using System;
using System.Collections.ObjectModel;
using System.DirectoryServices.Protocols;
using LinqToLdap.Mapping.PropertyMappings;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class StringCollectionPropertyMappingTest
    {
        private PropertyMappingArguments<StringCollectionPropertyMappingTest> _mappingArguments;

        [TestInitialize]
        public void SetUp()
        {
            _mappingArguments = new PropertyMappingArguments<StringCollectionPropertyMappingTest>
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
            var propertyMapping = new StringCollectionPropertyMapping<StringCollectionPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => propertyMapping.FormatValueToFilter(strings))
                .Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void FormatValueToFilter_String_ReturnsString()
        {
            //prepare
            var mapping = new StringCollectionPropertyMapping<StringCollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueToFilter("str");

            //assert
            value.Should().Be.EqualTo("str");
        }

        [TestMethod]
        public void FormatValueFromDirectory_StringArray_ReturnsStringCollection()
        {
            //prepare
            var strings = new[] { "one", "two", "three", "four" };
            var propertyMapping = new StringCollectionPropertyMapping<StringCollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("names", strings), "dn");

            //assert
            value.As<Collection<string>>().Should().Have.SameSequenceAs(strings);
        }

        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<string>);
            var propertyMapping = new StringCollectionPropertyMapping<StringCollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void FormatValueFromDirectory_SingleString_ReturnsAsStringArray()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<string>);
            var propertyMapping = new StringCollectionPropertyMapping<StringCollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", "str"), "dn");

            //assert
            value.As<Collection<string>>().Should().Contain("str");
        }

        [TestMethod]
        public void IsEqual_SameLengthDifferentCollections_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new Collection<string>(new Collection<string>(new string[] { "1", "2", "3" }));
            var propertyMapping = new StringCollectionPropertyMapping<StringCollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<string>(new string[] { "1", "2", "7" }), out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_DifferentLengths_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new Collection<string>(new string[] { "1", "2", "3" });
            var propertyMapping = new StringCollectionPropertyMapping<StringCollectionPropertyMappingTest>(_mappingArguments);
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
            var propertyMapping = new StringCollectionPropertyMapping<StringCollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<string>(new string[] { "1", "2", "3" }), out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_SameCollections_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => new Collection<string>(new string[] { "1", "2", "3" });
            var propertyMapping = new StringCollectionPropertyMapping<StringCollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<string>(new string[] { "1", "2", "3" }), out modification);

            //assert
            value.Should().Be.True();
            modification.Should().Be.Null();
        }
    }
}
