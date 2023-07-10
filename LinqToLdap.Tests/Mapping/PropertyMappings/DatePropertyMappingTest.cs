using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping.PropertyMappings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class DatePropertyMappingTest
    {
        private PropertyMappingArguments<DatePropertyMappingTest> _mappingArguments;

        [TestInitialize]
        public void SetUp()
        {
            _mappingArguments = new PropertyMappingArguments<DatePropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object),
                DirectoryMappings = new Dictionary<string, object>(),
                InstanceMappings = new Dictionary<object, string>()
            };
        }

        [TestMethod]
        public void FormatValueToFilter_FileTime_ReturnsFileTime()
        {
            //prepare
            var date = DateTime.Now;
            var mapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, null);

            //aact
            var value = mapping.FormatValueToFilter(date);

            //assert
            value.Should().Be(date.ToFileTime().ToString());
        }

        [TestMethod]
        public void FormatValueToFilter_DateTime_ReturnsDateTime()
        {
            //prepare
            var date = DateTime.Now;
            var mapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, "yyyyMMddHHmmss.0Z");

            //aact
            var value = mapping.FormatValueToFilter(date);

            //assert
            value.Should().Be(date.FormatLdapDateTime("yyyyMMddHHmmss.0Z"));
        }

        [TestMethod]
        public void FormatValueFromDirectory_NullAndNullable_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof (DateTime?);
            var mapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, "yyyyMMddHHmmss.0Z");

            //act
            var value = mapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().BeNull();
        }

        [TestMethod]
        public void FormatValueFromDirectory_NullAndNotNullable_ThrowsMappingException()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(DateTime);
            var mapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, "yyyyMMddHHmmss.0Z");

            //act
            Executing.This(() => mapping.FormatValueFromDirectory(null, "dn"))
                .Should().Throw<MappingException>();
        }

        [TestMethod]
        public void FormatValueFromDirectory_ZeroLengthAndNullable_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(DateTime?);
            var mapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, "yyyyMMddHHmmss.0Z");

            //act
            var value = mapping.FormatValueFromDirectory(new DirectoryAttribute("name"), "dn");

            //assert
            value.Should().BeNull();
        }

        [TestMethod]
        public void FormatValueFromDirectory_ZeroLengthAndNotNullable_ThrowsMappingException()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(DateTime);
            var mapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, "yyyyMMddHHmmss.0Z");

            //act
            Executing.This(() => mapping.FormatValueFromDirectory(new DirectoryAttribute("name"), "dn"))
                .Should().Throw<MappingException>();
        }

        [TestMethod]
        public void FormatValueFromDirectory_FileTime_ReturnsDateTimeFromFileTime()
        {
            //prepare
            var date = DateTime.Now;
            var mapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, null);

            //act
            var value = mapping.FormatValueFromDirectory(new DirectoryAttribute("name", date.ToFileTime().ToString()), "dn");

            //assert
            value.Should().Be(date);
        }

        [TestMethod]
        public void FormatValueFromDirectory_DateTime_ReturnsDateTimeFromDateTime()
        {
            //prepare
            var date = DateTime.Now;
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);
            var mapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, "yyyyMMddHHmmss.0Z");

            //act
            var value = mapping.FormatValueFromDirectory(new DirectoryAttribute("name", date.FormatLdapDateTime("yyyyMMddHHmmss.0Z")), "dn");

            //assert
            value.Should().Be(date);
        }

        [TestMethod]
        public void FormatValueFromDirectory_InvalidFomrat_CatchesExceptionAndRethrowsAsMappingException()
        {
            //prepare

            var mapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, null);

            //act
            Executing.This(() => mapping.FormatValueFromDirectory(new DirectoryAttribute("name", "a"), "dn"))
                .Should().Throw<MappingException>()
                .And.InnerException.Should().BeOfType<FormatException>();
        }

        [TestMethod]
        public void GetDirectoryAttributeModification_DateTime_ReturnsDateTime()
        {
            //prepare
            var now = DateTime.Now;
            _mappingArguments.Getter = t => now;
            var propertyMapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, ExtensionMethods.LdapFormat);

            //act
            var value = propertyMapping.GetDirectoryAttributeModification(this);

            //assert
            value.Name.Should().Be(_mappingArguments.AttributeName);
            value.Operation.Should().Be(DirectoryAttributeOperation.Replace);
            value[0].Should().Be(now.FormatLdapDateTime(ExtensionMethods.LdapFormat));
        }

        [TestMethod]
        public void GetDirectoryAttributeModification_FileTime_ReturnsFileTime()
        {
            //prepare
            var now = DateTime.Now;
            _mappingArguments.Getter = t => now;
            var propertyMapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, null);

            //act
            var value = propertyMapping.GetDirectoryAttributeModification(this);

            //assert
            value.Name.Should().Be(_mappingArguments.AttributeName);
            value.Operation.Should().Be(DirectoryAttributeOperation.Replace);
            value[0].Should().Be(now.ToFileTime().ToString());
        }

        [TestMethod]
        public void IsEqual_DiffferentDates_ReturnsFalse()
        {
            //prepare
            var date = DateTime.Now;
            _mappingArguments.Getter = t => DateTime.Now;
            var propertyMapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, null);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, date.AddDays(1), out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, null);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, DateTime.Now, out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_SameDateTimes_ReturnsTrue()
        {
            //prepare
            var now = DateTime.Now;
            _mappingArguments.Getter = t => now;
            var propertyMapping = new DatePropertyMapping<DatePropertyMappingTest>(_mappingArguments, null);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, now, out modification);

            //assert
            value.Should().BeTrue();
            modification.Should().BeNull();
        }
    }
}
