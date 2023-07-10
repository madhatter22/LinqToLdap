using System;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using LinqToLdap.Mapping.PropertyMappings;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class SecurityIdentifierArrayPropertyMappingTest
    {
        private PropertyMappingArguments<SecurityIdentifierArrayPropertyMappingTest> _mappingArguments;
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
            
            _identifier2 = WindowsIdentity.GetCurrent().User.AccountDomainSid;

            _mappingArguments = new PropertyMappingArguments<SecurityIdentifierArrayPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_SecurityIdentifierArray_ThrowsNotSupportedException()
        {
            //prepare
            var identifiers = new[] { _identifier, _identifier2 };
            var propertyMapping = new SecurityIdentifierArrayPropertyMapping<SecurityIdentifierArrayPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => propertyMapping.FormatValueToFilter(identifiers))
                .Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void FormatValueToFilter_SecurityIdentifier_ReturnsStringOctet()
        {
            //prepare
            var propertyMapping = new SecurityIdentifierArrayPropertyMapping<SecurityIdentifierArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueToFilter(_identifier);

            //assert
            value.Should().Be(_bytes.ToStringOctet());
        }

        [TestMethod]
        public void FormatValueFromDirectory_SecurityIdentifierArray_ReturnsSecurityIdentifierArray()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(SecurityIdentifier[]);
            var identifiers = new[] { _identifier, _identifier2 };
            var propertyMapping = new SecurityIdentifierArrayPropertyMapping<SecurityIdentifierArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", identifiers.Select(
                x =>
                {
                    var bytes = new byte[x.BinaryLength];
                    x.GetBinaryForm(bytes, 0);
                    return bytes;
                }).ToArray()), "dn");

            //assert
            value.CastTo<SecurityIdentifier[]>().Should().ContainInOrder(identifiers);
        }

        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(SecurityIdentifier[]);
            var propertyMapping = new SecurityIdentifierArrayPropertyMapping<SecurityIdentifierArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().BeNull();
        }

        [TestMethod]
        public void FormatValueFromDirectory_SingleSecurityIdentifier_ReturnsAsSecurityIdentifierArray()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(SecurityIdentifier[]);
            var propertyMapping = new SecurityIdentifierArrayPropertyMapping<SecurityIdentifierArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", _bytes), "dn");

            //assert
            value.CastTo<SecurityIdentifier[]>().Should().Contain(_identifier);
        }

        [TestMethod]
        public void IsEqual_SameLengthDifferentArrays_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new[] { _identifier, _identifier };
            var propertyMapping = new SecurityIdentifierArrayPropertyMapping<SecurityIdentifierArrayPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new[] { _identifier, _identifier2 }, out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_DifferentLengths_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new[] { _identifier, _identifier2 };
            var propertyMapping = new SecurityIdentifierArrayPropertyMapping<SecurityIdentifierArrayPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new[] { _identifier }, out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new SecurityIdentifierArrayPropertyMapping<SecurityIdentifierArrayPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new[] { _identifier }, out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_SameArrays_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => new[] { _identifier, _identifier2 };
            var propertyMapping = new SecurityIdentifierArrayPropertyMapping<SecurityIdentifierArrayPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new[] { _identifier, _identifier2 }, out modification);

            //assert
            value.Should().BeTrue();
            modification.Should().BeNull();
        }
    }
}
