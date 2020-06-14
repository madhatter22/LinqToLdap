using LinqToLdap.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using System;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.Tests.Mapping
{
    internal class PropertyMappingTestClass : PropertyMapping
    {
        public PropertyMappingTestClass(Type propertyType, string propertyName, string attributeName, bool isDistinguishedName = false, ReadOnly readOnly = ReadOnly.Never)
            : base(propertyType, propertyName, attributeName, isDistinguishedName, readOnly)
        {
        }

        public override object GetValue(object instance)
        {
            throw new NotSupportedException();
        }

        public override void SetValue(object instance, object value)
        {
        }

        public override object FormatValueFromDirectory(DirectoryAttribute value, string dn)
        {
            throw new NotImplementedException();
        }

        public override string FormatValueToFilter(object value)
        {
            throw new NotImplementedException();
        }

        public override DirectoryAttributeModification GetDirectoryAttributeModification(object instance)
        {
            throw new NotImplementedException();
        }

        public override object GetValueForDirectory(object instance)
        {
            throw new NotImplementedException();
        }
    }

    [TestClass]
    public class PropertyMappingTest
    {
        [TestMethod]
        public void PropertyMapping_String_MapsCorrectly()
        {
            //act
            var mapping = new PropertyMappingTestClass(typeof(string), "prop", "att");

            //assert
            mapping.AttributeName.Should().Be.EqualTo("att");
            mapping.PropertyName.Should().Be.Equals("prop");
            mapping.PropertyType.Should().Be.EqualTo(typeof(string));
            mapping.IsNullable.Should().Be.True();
        }

        [TestMethod]
        public void PropertyMapping_StringArray_MapsAsNullable()
        {
            //act
            var mapping = new PropertyMappingTestClass(typeof(string[]), "prop", "prop");

            //assert
            mapping.IsNullable.Should().Be.True();
        }

        [TestMethod]
        public void PropertyMapping_ByteArray_MapsAsNullable()
        {
            //act
            var mapping = new PropertyMappingTestClass(typeof(byte[]), "prop", "prop");

            //assert
            mapping.IsNullable.Should().Be.True();
        }

        [TestMethod]
        public void PropertyMapping_NullableGuid_MapsAsNullable()
        {
            //act
            var mapping = new PropertyMappingTestClass(typeof(Guid?), "prop", "prop");

            //assert
            mapping.IsNullable.Should().Be.True();
        }

        [TestMethod]
        public void PropertyMapping_Guid_MapsAsNotNullable()
        {
            //act
            var mapping = new PropertyMappingTestClass(typeof(Guid), "prop", "prop");

            //assert
            mapping.IsNullable.Should().Be.False();
        }

        [TestMethod]
        public void Default_NullableValueType_ReturnsNull()
        {
            //prepare
            var mapping = new PropertyMappingTestClass(typeof(DateTime?), "prop", "prop");

            //act
            var value = mapping.Default();

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void Default_ValueType_ReturnsDefault()
        {
            //prepare
            var mapping = new PropertyMappingTestClass(typeof(DateTime), "prop", "prop");

            //act
            var value = mapping.Default();

            //assert
            value.Should().Be.EqualTo(default(DateTime));
        }

        [TestMethod]
        public void Default_ObjectType_ReturnsNull()
        {
            //prepare
            var mapping = new PropertyMappingTestClass(typeof(string[]), "prop", "prop");

            //act
            var value = mapping.Default();

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void Default_ObjectType_SetsDefaultMethod()
        {
            //prepare
            var mapping = new PropertyMappingTestClass(typeof(string[]), "prop", "prop");
            var mapping2 = new PropertyMappingTestClass(typeof(string[]), "prop2", "prop2");

            //act
            var value = mapping.Default();
            var value2 = mapping2.Default();

            //assert
            value.Should().Be.Null();
            value2.Should().Be.Null();
        }
    }
}