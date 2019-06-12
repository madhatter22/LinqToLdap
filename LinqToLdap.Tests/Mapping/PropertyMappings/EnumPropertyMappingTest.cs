using System;
using System.DirectoryServices.Protocols;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping.PropertyMappings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class EnumPropertyMappingTest
    {
        private enum EnumTest
        {
            Value1 = 1,
            Value2 = 2
        }

        private PropertyMappingArguments<EnumPropertyMappingTest> _mappingArguments;

        [TestInitialize]
        public void SetUp()
        {
            _mappingArguments = new PropertyMappingArguments<EnumPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(EnumTest)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_MappedAsInt_ReturnsIntString()
        {
            //prepare
            var mapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, true);

            //act
            var value = mapping.FormatValueToFilter(EnumTest.Value2);

            //assert
            value.Should().Be.EqualTo("2");
        }

        [TestMethod]
        public void FormatValueToFilter_MappedAsIntButReceivesString_ReturnsIntString()
        {
            //prepare
            var mapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, true);

            //act
            var value = mapping.FormatValueToFilter(EnumTest.Value2);

            //assert
            value.Should().Be.EqualTo("2");
        }

        [TestMethod]
        public void FormatValueToFilter_MappedAsString_ReturnsString()
        {
            //prepare
            var mapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, false);

            //act
            var value = mapping.FormatValueToFilter(EnumTest.Value2);

            //assert
            value.Should().Be.EqualTo("Value2");
        }

        [TestMethod]
        public void FormatValueToFilter_MappedAsStringButReceivesInt_ReturnsString()
        {
            //prepare
            var mapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, false);

            //act
            var value = mapping.FormatValueToFilter((int)EnumTest.Value2);

            //assert
            value.Should().Be.EqualTo("Value2");
        }

        [TestMethod]
        public void FormatValueToFilter_MappedAsStringButReceivesNullableInt_ReturnsString()
        {
            //prepare
            var mapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, false);

            //act
            var value = mapping.FormatValueToFilter((int?)EnumTest.Value2);

            //assert
            value.Should().Be.EqualTo("Value2");
        }

        [TestMethod]
        public void FormatValueFromDirectory_NullAndNullable_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(EnumTest?);
            var mapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, false);

            //act
            var value = mapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void FormatValueFromDirectory_NullAndNotNullable_ThrowsMappingException()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(EnumTest);
            var mapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, false);

            //act
            Executing.This(() => mapping.FormatValueFromDirectory(null, "dn"))
                .Should().Throw<MappingException>();
        }

        [TestMethod]
        public void FormatValueFromDirectory_ZeroLengthAndNullable_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(EnumTest?);
            var mapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, false);

            //act
            var value = mapping.FormatValueFromDirectory(new DirectoryAttribute("name"), "dn");

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void FormatValueFromDirectory_ZeroLengthAndNotNullable_ThrowsMappingException()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(EnumTest);
            var mapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, false);

            //act
            Executing.This(() => mapping.FormatValueFromDirectory(new DirectoryAttribute("name"), "dn"))
                .Should().Throw<MappingException>();
        }

        [TestMethod]
        public void FormatValueFromDirectory_MappedAsInt_ReturnsEnumTestFromInt()
        {
            //prepare
            var mapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, true);

            //act
            var value = mapping.FormatValueFromDirectory(new DirectoryAttribute("name", "2"), "dn");

            //assert
            value.Should().Be.EqualTo(EnumTest.Value2);
        }

        [TestMethod]
        public void FormatValueFromDirectory_MappedAsString_ReturnsEnumTestFromString()
        {
            //prepare
            var mapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, false);

            //act
            var value = mapping.FormatValueFromDirectory(new DirectoryAttribute("name", "Value2"), "dn");

            //assert
            value.Should().Be.EqualTo(EnumTest.Value2);
        }

        [TestMethod]
        public void FormatValueFromDirectory_InvalidStringFomrat_CatchesExceptionAndRethrowsAsMappingException()
        {
            //prepare
            var mapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, false);

            //act
            Executing.This(() => mapping.FormatValueFromDirectory(new DirectoryAttribute("name", "a"), "dn"))
                .Should().Throw<MappingException>().And.Exception
                .Satisfies(e => e.InnerException.As<ArgumentException>() != null);
        }

        [TestMethod]
        public void GetDirectoryAttributeModification_EnumAsInt_ReturnsIntString()
        {
            //prepare
            _mappingArguments.Getter = t => EnumTest.Value2;
            var propertyMapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, true);

            //act
            var value = propertyMapping.GetDirectoryAttributeModification(this);

            //assert
            value.Name.Should().Be.EqualTo(_mappingArguments.AttributeName);
            value.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
            value[0].Should().Be.EqualTo("2");
        }

        [TestMethod]
        public void GetDirectoryAttributeModification_EnumAsString_ReturnsString()
        {
            //prepare
            _mappingArguments.Getter = t => EnumTest.Value2;
            var propertyMapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, false);

            //act
            var value = propertyMapping.GetDirectoryAttributeModification(this);

            //assert
            value.Name.Should().Be.EqualTo(_mappingArguments.AttributeName);
            value.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
            value[0].Should().Be.EqualTo(EnumTest.Value2.ToString());
        }

        [TestMethod]
        public void IsEqual_DiffferentEnums_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => EnumTest.Value1;
            var propertyMapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, true);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, EnumTest.Value2, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, false);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, EnumTest.Value2, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_SameDateTimes_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => EnumTest.Value2;
            var propertyMapping = new EnumPropertyMapping<EnumPropertyMappingTest>(_mappingArguments, false);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, EnumTest.Value2, out modification);

            //assert
            value.Should().Be.True();
            modification.Should().Be.Null();
        }
    }
}
