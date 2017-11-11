using System;
using System.Collections.ObjectModel;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Principal;
using LinqToLdap.Mapping.PropertyMappings;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class SecurityIdentifierCollectionPropertyMappingTest
    {
        private PropertyMappingArguments<SecurityIdentifierCollectionPropertyMappingTest> _mappingArguments;
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

            _mappingArguments = new PropertyMappingArguments<SecurityIdentifierCollectionPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_SecurityIdentifierCollection_ThrowsNotSupportedException()
        {
            //prepare
            var identifiers = new Collection<SecurityIdentifier>(new[] { _identifier, _identifier2 });
            var propertyMapping = new SecurityIdentifierCollectionPropertyMapping<SecurityIdentifierCollectionPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => propertyMapping.FormatValueToFilter(identifiers))
                .Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void FormatValueToFilter_SecurityIdentifier_ReturnsStringOctet()
        {
            //prepare
            var propertyMapping = new SecurityIdentifierCollectionPropertyMapping<SecurityIdentifierCollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueToFilter(_identifier);

            //assert
            value.Should().Be.EqualTo(_bytes.ToStringOctet());
        }

        [TestMethod]
        public void FormatValueFromDirectory_SecurityIdentifierCollection_ReturnsSecurityIdentifierCollection()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<SecurityIdentifier>);
            var identifiers = new Collection<SecurityIdentifier>(new[] { _identifier, _identifier2 });
            var propertyMapping = new SecurityIdentifierCollectionPropertyMapping<SecurityIdentifierCollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", identifiers.Select(
                x =>
                {
                    var bytes = new byte[x.BinaryLength];
                    x.GetBinaryForm(bytes, 0);
                    return bytes;
                }).ToArray()), "dn");

            //assert
            value.As<Collection<SecurityIdentifier>>().Should().Have.SameSequenceAs(identifiers);
        }

        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<SecurityIdentifier>);
            var propertyMapping = new SecurityIdentifierCollectionPropertyMapping<SecurityIdentifierCollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void FormatValueFromDirectory_SingleSecurityIdentifier_ReturnsAsSecurityIdentifierCollection()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<SecurityIdentifier>);
            var propertyMapping = new SecurityIdentifierCollectionPropertyMapping<SecurityIdentifierCollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", _bytes), "dn");

            //assert
            value.As<Collection<SecurityIdentifier>>().Should().Contain(_identifier);
        }

        [TestMethod]
        public void IsEqual_SameLengthDifferentCollections_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new Collection<SecurityIdentifier>(new[] { _identifier, _identifier });
            var propertyMapping = new SecurityIdentifierCollectionPropertyMapping<SecurityIdentifierCollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<SecurityIdentifier>(new[] { _identifier, _identifier2 }), out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_DifferentLengths_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new Collection<SecurityIdentifier>(new[] { _identifier, _identifier2 });
            var propertyMapping = new SecurityIdentifierCollectionPropertyMapping<SecurityIdentifierCollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<SecurityIdentifier>(new[] { _identifier }), out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new SecurityIdentifierCollectionPropertyMapping<SecurityIdentifierCollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<SecurityIdentifier>(new[] { _identifier }), out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_SameCollections_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => new Collection<SecurityIdentifier>(new[] { _identifier, _identifier2 });
            var propertyMapping = new SecurityIdentifierCollectionPropertyMapping<SecurityIdentifierCollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<SecurityIdentifier>(new[] { _identifier, _identifier2 }), out modification);

            //assert
            value.Should().Be.True();
            modification.Should().Be.Null();
        }
    }
}
