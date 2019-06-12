using System;
using System.DirectoryServices.Protocols;
using LinqToLdap.Mapping.PropertyMappings;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class CustomPropertyMappingTest
    {
        private PropertyMappingArguments<CustomPropertyMappingTest> _mappingArguments;
        private Guid _guid;
        private Func<DirectoryAttribute, Guid> _convertFrom;
        private Func<Guid, object> _convertTo;
        private Func<Guid, string> _convertToFilter;
        private Func<Guid, Guid, bool> _compareTo;

        [TestInitialize]
        public void SetUp()
        {
            _guid = new Guid(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 });
            _convertFrom = d =>
                               {
                                   var bytes = d.GetValues(typeof (byte[]))[0] as byte[];
                                   return new Guid(bytes);
                               };
            _convertToFilter = v => v.ToStringOctet();
            _convertTo = v => v.ToByteArray();
            _compareTo = (v1, v2) => v1 == v2;
            _mappingArguments = new PropertyMappingArguments<CustomPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_Guid_ReturnsStringOctet()
        {
            //prepare
            var propertyMapping = new CustomPropertyMapping<CustomPropertyMappingTest, Guid>(_mappingArguments, _convertFrom, _convertTo, _convertToFilter, _compareTo);

            //act
            var value = propertyMapping.FormatValueToFilter(_guid);

            //assert
            value.Should().Be.EqualTo(_guid.ToStringOctet());
        }

        [TestMethod]
        public void FormatValueFromDirectory_ByteArray_ReturnsGuid()
        {
            //prepare
            var propertyMapping = new CustomPropertyMapping<CustomPropertyMappingTest, Guid>(_mappingArguments, _convertFrom, _convertTo, _convertToFilter, _compareTo);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", _guid.ToByteArray()), "dn");

            //assert
            value.CastTo<Guid>().ToByteArray().Should().Have.SameSequenceAs(_guid.ToByteArray());
        }

        [TestMethod]
        public void GetDirectoryAttributeModification_ByteArray_ReturnsByteArray()
        {
            //prepare
            _mappingArguments.Getter = d => _guid;
            var propertyMapping = new CustomPropertyMapping<CustomPropertyMappingTest, Guid>(_mappingArguments, _convertFrom, _convertTo, _convertToFilter, _compareTo);

            //act
            var value = propertyMapping.GetDirectoryAttributeModification(this);

            //assert
            value.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
            value.Name.Should().Be.EqualTo(_mappingArguments.AttributeName);
            value[0].As<byte[]>().Should().Have.SameSequenceAs(_guid.ToByteArray());
        }

        [TestMethod]
        public void GetDirectoryAttributeModification_String_ReturnsString()
        {
            //prepare
            _mappingArguments.Getter = d => "str";
            Func<string, object> convertTo = s => "convert";
            var propertyMapping = new CustomPropertyMapping<CustomPropertyMappingTest, string>(_mappingArguments, null, convertTo, null, null);

            //act
            var value = propertyMapping.GetDirectoryAttributeModification(this);

            //assert
            value.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
            value.Name.Should().Be.EqualTo(_mappingArguments.AttributeName);
            value[0].As<string>().Should().Be.EqualTo("convert");
        }

        [TestMethod]
        public void GetDirectoryAttributeModification_StringArray_ReturnsStringArray()
        {
            //prepare
            _mappingArguments.Getter = d => new []{"x"};
            Func<string[], object> convertTo = s => new[]{"1", "2"};
            var propertyMapping = new CustomPropertyMapping<CustomPropertyMappingTest, string[]>(_mappingArguments, null, convertTo, null, null);

            //act
            var value = propertyMapping.GetDirectoryAttributeModification(this);

            //assert
            value.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
            value.Name.Should().Be.EqualTo(_mappingArguments.AttributeName);
            value.GetValues(typeof(string)).As<string[]>().Should().Have.SameSequenceAs(new[] {"1", "2"});
        }

        [TestMethod]
        public void GetDirectoryAttributeModification_ByteArrayArray_ReturnsByteArrayArray()
        {
            //prepare
            _mappingArguments.Getter = d => new[] { _guid.ToByteArray() };
            Func<byte[][], object> convertTo = s => new[] { _guid.ToByteArray(), _guid.ToByteArray() };
            var propertyMapping = new CustomPropertyMapping<CustomPropertyMappingTest, byte[][]>(_mappingArguments, null, convertTo, null, null);

            //act
            var value = propertyMapping.GetDirectoryAttributeModification(this);

            //assert
            value.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
            value.Name.Should().Be.EqualTo(_mappingArguments.AttributeName);
            value.GetValues(typeof(byte[])).As<byte[][]>()[0].Should().Have.SameSequenceAs(_guid.ToByteArray());
            value.GetValues(typeof(byte[])).As<byte[][]>()[1].Should().Have.SameSequenceAs(_guid.ToByteArray());
        }

        [TestMethod]
        public void IsEqual_EqualFuncReturnsFalseButEqualGuids_ReturnsFalse()
        {
            //prepare
            Func<Guid, Guid, bool> isEqual = (g1, g2) => false;
            _mappingArguments.Getter = t => _guid;
            var propertyMapping = new CustomPropertyMapping<CustomPropertyMappingTest, Guid>(_mappingArguments, null, _convertTo, null, isEqual);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, _guid, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_NullEqualFuncReturnsBaseValueWithEqualGuids_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => _guid;
            var propertyMapping = new CustomPropertyMapping<CustomPropertyMappingTest, Guid>(_mappingArguments, null, _convertTo, null, null);
            DirectoryAttributeModification modification;

            //act
            var value = propertyMapping.IsEqual(this, _guid, out modification);

            //assert
            value.Should().Be.True();
            modification.Should().Be.Null();
        }

        [TestMethod]
        public void IsEqual_BothNull_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new CustomPropertyMapping<CustomPropertyMappingTest, Guid>(_mappingArguments, null, _convertTo, null, null);
            DirectoryAttributeModification modification;

            //act
            var value = propertyMapping.IsEqual(this, null, out modification);

            //assert
            value.Should().Be.True();
            modification.Should().Be.Null();
        }
    }
}
