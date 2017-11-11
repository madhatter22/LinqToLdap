using System.DirectoryServices.Protocols;
using System.Security.Principal;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping.PropertyMappings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class SecurityIdentifierPropertyMappingTest
    {
        private PropertyMappingArguments<SecurityIdentifierPropertyMappingTest> _mappingArguments;
        private SecurityIdentifier _identifier;
        private SecurityIdentifier _identifier2;
        private byte[] _bytes;

        [TestInitialize]
        public void SetUp()
        {
            _bytes = new byte[]
                         {
                             1, 5, 0, 0, 27, 14, 3, 139, 251, 73, 97, 48, 157, 6, 235, 192, 201, 125, 33,
                             65, 182, 209, 6, 82, 206, 165, 32, 24
                         };
            _identifier =
                new SecurityIdentifier(_bytes, 0);

            _identifier2 = new SecurityIdentifier(new byte[]
                                                      {
                                                          1,5,0,0,24,231,136,168,3,161,50,243,78,221,38,
                                                          69,104,65,224,78,188,248,211,212,60,19,48,228,
                                                      }, 0);

            _mappingArguments = new PropertyMappingArguments<SecurityIdentifierPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_SecurityIdentifier_ReturnsStringOctet()
        {
            //prepare
            var propertyMapping = new SecurityIdentifierPropertyMapping<SecurityIdentifierPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueToFilter(_identifier);

            //assert
            value.Should().Be.EqualTo(_bytes.ToStringOctet());
        }

        [TestMethod]
        public void FormatValueFromDirectory_SecurityIdentifier_ReturnsSecurityIdentifier()
        {
            //prepare
            var propertyMapping = new SecurityIdentifierPropertyMapping<SecurityIdentifierPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", _bytes), "dn");

            //assert
            value.Should().Be.EqualTo(_identifier);
        }

        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(byte[]);
            var propertyMapping = new SecurityIdentifierPropertyMapping<SecurityIdentifierPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void IsEqual_DiffferentSecurityIdentifiers_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => _identifier;
            var propertyMapping = new SecurityIdentifierPropertyMapping<SecurityIdentifierPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;

            //act
            var value = propertyMapping.IsEqual(this, _identifier2, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new SecurityIdentifierPropertyMapping<SecurityIdentifierPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, _identifier2, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_SameSecurityIdentifiers_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => new SecurityIdentifier(_bytes, 0);
            var propertyMapping = new SecurityIdentifierPropertyMapping<SecurityIdentifierPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, _identifier, out modification);

            //assert
            value.Should().Be.True();
            modification.Should().Be.Null();
        }
    }
}
