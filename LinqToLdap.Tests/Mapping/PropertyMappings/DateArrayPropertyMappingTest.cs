using System;
using System.DirectoryServices.Protocols;
using System.Linq;
using LinqToLdap.Mapping.PropertyMappings;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class DateArrayPropertyMappingTest
    {
        private PropertyMappingArguments<DateArrayPropertyMappingTest> _mappingArguments;

        [TestInitialize]
        public void SetUp()
        {
            _mappingArguments = new PropertyMappingArguments<DateArrayPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_FileTimeDateTimeArray_ThrowsNotSupportedException()
        {
            //prepare
            var dates = new[] { DateTime.Now, DateTime.Now.AddDays(1) };
            var propertyMapping = new DateArrayPropertyMapping<DateArrayPropertyMappingTest>(_mappingArguments, null);

            //act
            Executing.This(() => propertyMapping.FormatValueToFilter(dates))
                .Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void FormatValueToFilter_DateTime_ReturnsDateTime()
        {
            //prepare
            var date = DateTime.Now;
            var mapping = new DatePropertyMapping<DateArrayPropertyMappingTest>(_mappingArguments, "yyyyMMddHHmmss.0Z");

            //act
            var value = mapping.FormatValueToFilter(date);

            //assert
            value.Should().Be(date.FormatLdapDateTime("yyyyMMddHHmmss.0Z"));
        }

        [TestMethod]
        public void FormatValueFromDirectory_NullableFileTimeDateTimeArray_ReturnsDateTimesArray()
        {
            //prepare
            var dates = new DateTime?[] { DateTime.Now, DateTime.Now.AddDays(1) };
            _mappingArguments.PropertyType = typeof (DateTime?[]);
            var propertyMapping = new DateArrayPropertyMapping<DateArrayPropertyMappingTest>(_mappingArguments, null);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("names", dates.Select(d => d.Value.ToFileTime().ToString()).ToArray()), "dn");

            //assert
            value.CastTo<DateTime?[]>().Should().ContainInOrder(dates);
        }

        [TestMethod]
        public void FormatValueFromDirectory_DateTimeArray_ReturnsDateTimesArray()
        {
            //prepare
            var now = DateTime.Now;
            var tomorrow = now.AddDays(1);
            var dates = new[] { new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second), 
                new DateTime(tomorrow.Year, tomorrow.Month, tomorrow.Day, tomorrow.Hour, tomorrow.Minute, tomorrow.Second) };
            _mappingArguments.PropertyType = typeof(DateTime[]);
            var propertyMapping = new DateArrayPropertyMapping<DateArrayPropertyMappingTest>(_mappingArguments, "yyyyMMddHHmmss.0Z");

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("names", dates.Select(d => d.FormatLdapDateTime("yyyyMMddHHmmss.0Z")).ToArray()), "dn");

            //assert
            value.CastTo<DateTime[]>().Should().ContainInOrder(dates);
        }

        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(DateTime[]);
            var propertyMapping = new DateArrayPropertyMapping<DateArrayPropertyMappingTest>(_mappingArguments, null);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().BeNull();
        }

        [TestMethod]
        public void FormatValueFromDirectory_SingleDateTime_ReturnsAsDateTimeArray()
        {
            //prepare
            var now = DateTime.Now;
            _mappingArguments.PropertyType = typeof(DateTime[]);
            var propertyMapping = new DateArrayPropertyMapping<DateArrayPropertyMappingTest>(_mappingArguments, null);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", now.ToFileTime().ToString()), "dn");

            //assert
            value.CastTo<DateTime[]>().Should().Contain(now);
        }

        [TestMethod]
        public void IsEqual_SameLengthDifferentArrays_ReturnsFalse()
        {
            //prepare
            var date1 = DateTime.Now;
            var date2 = DateTime.Now.AddDays(1);
            var date3 = DateTime.Now.AddDays(2);
            _mappingArguments.Getter = t => new [] { date1, date2, date3 };
            var propertyMapping = new DateArrayPropertyMapping<DateArrayPropertyMappingTest>(_mappingArguments, null);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new [] { date1, date2, date3.AddDays(1) }, out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_DifferentLengths_ReturnsFalse()
        {
            //prepare
            var date1 = DateTime.Now;
            var date2 = DateTime.Now.AddDays(1);
            var date3 = DateTime.Now.AddDays(2);
            _mappingArguments.Getter = t => new[] { date1, date2, date3 };
            var propertyMapping = new DateArrayPropertyMapping<DateArrayPropertyMappingTest>(_mappingArguments, null);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new[] { date1, date2 }, out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new DateArrayPropertyMapping<DateArrayPropertyMappingTest>(_mappingArguments, null);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new[] { DateTime.Now }, out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_SameArrays_ReturnsTrue()
        {
            //prepare
            var date1 = DateTime.Now;
            var date2 = DateTime.Now.AddDays(1);
            var date3 = DateTime.Now.AddDays(2);
            _mappingArguments.Getter = t => new[] { date1, date2, date3 };
            _mappingArguments.PropertyType = typeof (DateTime[]);
            var propertyMapping = new DateArrayPropertyMapping<DateArrayPropertyMappingTest>(_mappingArguments, null);
            DirectoryAttributeModification modification;

            //act
            var value = propertyMapping.IsEqual(this, new[] { date1, date2, date3 }, out modification);

            //assert
            value.Should().BeTrue();
            modification.Should().BeNull();
        }
    }
}
