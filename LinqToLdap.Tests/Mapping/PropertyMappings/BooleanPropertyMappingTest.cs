﻿using System.DirectoryServices.Protocols;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping.PropertyMappings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class BooleanPropertyMappingTest
    {
        private PropertyMappingArguments<BooleanPropertyMappingTest> _mappingArguments;

        [TestInitialize]
        public void SetUp()
        {
            _mappingArguments = new PropertyMappingArguments<BooleanPropertyMappingTest>
                                    {
                                        AttributeName = "att",
                                        PropertyName = "name",
                                        PropertyType = typeof (object)
                                    };
        }

        [TestMethod]
        public void FormatValueToFilter_True_ReturnsTrue()
        {
            //prepare
            var propertyMapping = new BooleanPropertyMapping<BooleanPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueToFilter(true);

            //assert
            value.Should().Be("TRUE");
        }

        [TestMethod]
        public void FormatValueToFilter_False_ReturnsFalse()
        {
            //prepare
            var propertyMapping = new BooleanPropertyMapping<BooleanPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueToFilter(false);

            //assert
            value.Should().Be("FALSE");
        }

        [TestMethod]
        public void FormatValueFromDirectory_True_ReturnsSameValue()
        {
            //prepare
            var propertyMapping = new BooleanPropertyMapping<BooleanPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", "TRUE"), "dn");

            //assert
            value.Should().Be(true);
        }

        [TestMethod]
        public void FormatValueFromDirectory_False_ReturnsSameValue()
        {
            //prepare
            var propertyMapping = new BooleanPropertyMapping<BooleanPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", "FALSE"), "dn");

            //assert
            value.Should().Be(false);
        }

        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsSameValue()
        {
            //prepare
            _mappingArguments.PropertyType = typeof (bool?);
            var propertyMapping = new BooleanPropertyMapping<BooleanPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().BeNull();
        }

        [TestMethod]
        public void FormatValueFromDirectory_NullButNotNullable_ThrowsMappingException()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(bool);
            var propertyMapping = new BooleanPropertyMapping<BooleanPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => propertyMapping.FormatValueFromDirectory(null, "dn"))
                .Should().Throw<MappingException>();
        }

        [TestMethod]
        public void GetDirectoryAttributeModification_True_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => true;
            var propertyMapping = new BooleanPropertyMapping<BooleanPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.GetDirectoryAttributeModification(this);

            //assert
            value.Name.Should().Be(_mappingArguments.AttributeName);
            value.Operation.Should().Be(DirectoryAttributeOperation.Replace);
            value[0].Should().Be("TRUE");
        }

        [TestMethod]
        public void GetDirectoryAttributeModification_False_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => false;
            var propertyMapping = new BooleanPropertyMapping<BooleanPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.GetDirectoryAttributeModification(this);

            //assert
            value.Name.Should().Be(_mappingArguments.AttributeName);
            value.Operation.Should().Be(DirectoryAttributeOperation.Replace);
            value[0].Should().Be("FALSE");
        }

        [TestMethod]
        public void IsEqual_BothDifferentBooleans_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => false;
            var propertyMapping = new BooleanPropertyMapping<BooleanPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, true, out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_BothSameBooleans_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => true;
            var propertyMapping = new BooleanPropertyMapping<BooleanPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, true, out modification);

            //assert
            value.Should().BeTrue();
            modification.Should().BeNull();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new BooleanPropertyMapping<BooleanPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, true, out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }
    }
}
