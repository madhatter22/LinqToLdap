using System;
using System.DirectoryServices.Protocols;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping.PropertyMappings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class NumericPropertyMappingTest
    {
        private PropertyMappingArguments<NumericPropertyMappingTest> _mappingArguments;

        [TestInitialize]
        public void SetUp()
        {
            _mappingArguments = new PropertyMappingArguments<NumericPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_Int_ReturnsString()
        {
            //prepare
            var mapping = new NumericPropertyMapping<NumericPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueToFilter(3);

            //assert
            value.Should().Be.EqualTo("3");
        }

        [TestMethod]
        public void FormatValueFromDirectory_Int_ReturnsInt()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(int);
            var mapping = new NumericPropertyMapping<NumericPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueFromDirectory(new DirectoryAttribute("name", "2"), "dn");

            //assert
            value.Should().Be.EqualTo(2);
        }

        [TestMethod]
        public void FormatValueFromDirectory_NullableInt_ReturnsInt()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(int?);
            var mapping = new NumericPropertyMapping<NumericPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueFromDirectory(new DirectoryAttribute("name", "2"), "dn");

            //assert
            value.Should().Be.EqualTo(2);
        }

        [TestMethod]
        public void FormatValueFromDirectory_NullableIntWithNull_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(int?);
            var mapping = new NumericPropertyMapping<NumericPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void FormatValueFromDirectory_NotNullableIntWithNull_ThrowsMappingException()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(int);
            var mapping = new NumericPropertyMapping<NumericPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => mapping.FormatValueFromDirectory(null, "dn"))
                .Should().Throw<MappingException>();
        }

        [TestMethod]
        public void FormatValueFromDirectory_NullableIntWithZeroLength_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(int?);
            var mapping = new NumericPropertyMapping<NumericPropertyMappingTest>(_mappingArguments);

            //act
            var value = mapping.FormatValueFromDirectory(new DirectoryAttribute("name"), "dn");

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void FormatValueFromDirectory_NotNullableIntWithZeroLength_ThrowsMappingException()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(int);
            var mapping = new NumericPropertyMapping<NumericPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => mapping.FormatValueFromDirectory(new DirectoryAttribute("name"), "dn"))
                .Should().Throw<MappingException>();
        }

        [TestMethod]
        public void FormatValueFromDirectory_InvalidFormat_CatchesExceptionAndRethrowsMappingException()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(int);
            var mapping = new NumericPropertyMapping<NumericPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => mapping.FormatValueFromDirectory(new DirectoryAttribute("name", "a"), "dn"))
                .Should().Throw<MappingException>().And.Exception.InnerException.Should().Be.InstanceOf<FormatException>();
        }

        [TestMethod]
        public void GetDirectoryAttributeModification_Numeric_ReturnsIntString()
        {
            //prepare
            _mappingArguments.Getter = t => 2;
            var propertyMapping = new NumericPropertyMapping<NumericPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.GetDirectoryAttributeModification(this);

            //assert
            value.Name.Should().Be.EqualTo(_mappingArguments.AttributeName);
            value.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
            value[0].Should().Be.EqualTo("2");
        }

        [TestMethod]
        public void IsEqual_DiffferentValues_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => 2;
            var propertyMapping = new NumericPropertyMapping<NumericPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;

            //act
            var value = propertyMapping.IsEqual(this, 1, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new NumericPropertyMapping<NumericPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, 2, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_SameValues_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => 4;
            var propertyMapping = new NumericPropertyMapping<NumericPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;

            //act
            var value = propertyMapping.IsEqual(this, 4, out modification);

            //assert
            value.Should().Be.True();
            modification.Should().Be.Null();
        }
    }
}
