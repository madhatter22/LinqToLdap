﻿using System;
using System.Collections.ObjectModel;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using LinqToLdap.Mapping.PropertyMappings;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

#if NET35
            using LinqToLdap.NET35.Tests.Properties;
#endif
#if NET40
            using LinqToLdap.NET40.Tests.Properties;
#endif
#if NET45

using LinqToLdap.NET45.Tests.Properties;

#endif
#if (!NET35 && !NET40 && !NET45)

using LinqToLdap.Tests.Properties;

#endif

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class X509Certificate2CollectionPropertyMappingTest
    {
        private PropertyMappingArguments<X509Certificate2CollectionPropertyMappingTest> _mappingArguments;

        [TestInitialize]
        public void SetUp()
        {
            _mappingArguments = new PropertyMappingArguments<X509Certificate2CollectionPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_X509Certificate2Collection_ThrowsNotSupportedException()
        {
            //prepare
            var certs = new Collection<X509Certificate2>(new[] { new X509Certificate2(Resources.cert), new X509Certificate2(Resources.cert) });
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => propertyMapping.FormatValueToFilter(certs))
                .Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void FormatValueToFilter_X509CertificateCollection_ThrowsNotSupportedException()
        {
            //prepare
            var certs = new[] { new X509Certificate(Resources.cert), new X509Certificate(Resources.cert) };
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => propertyMapping.FormatValueToFilter(certs))
                .Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void FormatValueToFilter_X509Certificate2_ReturnsStringOctet()
        {
            //prepare
            var certs = new X509Certificate2(Resources.cert);
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueToFilter(certs);

            //assert
            value.Should().Be(certs.GetRawCertData().ToStringOctet());
        }

        [TestMethod]
        public void FormatValueToFilter_X509Certificate_ReturnsStringOctet()
        {
            //prepare
            var certs = new X509Certificate(Resources.cert);
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueToFilter(certs);

            //assert
            value.Should().Be(certs.GetRawCertData().ToStringOctet());
        }

        [TestMethod]
        public void FormatValueFromDirectory_X509Certificate2Collection_ReturnsX509Certificate2Collection()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<X509Certificate2>);
            var certs = new Collection<X509Certificate2>(new[] { new X509Certificate2(Resources.cert), new X509Certificate2(Resources.cert) });
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", certs.Select(c => c.GetRawCertData()).ToArray()), "dn");

            //assert
            value.CastTo<Collection<X509Certificate2>>().Should().ContainInOrder(certs);
        }

        [TestMethod]
        public void FormatValueFromDirectory_X509CertificateCollection_ReturnsX509CertificateCollection()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<X509Certificate>);
            var certs = new[] { new X509Certificate(Resources.cert), new X509Certificate(Resources.cert) };
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", certs.Select(c => c.GetRawCertData()).ToArray()), "dn");

            //assert
            value.CastTo<Collection<X509Certificate>>().Should().ContainInOrder(certs);
        }

        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<X509Certificate2>);
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().BeNull();
        }

        [TestMethod]
        public void FormatValueFromDirectory_SingleX509Certificate2_ReturnsAsX509Certificate2Collection()
        {
            //prepare
            var certs = new X509Certificate2(Resources.cert);
            _mappingArguments.PropertyType = typeof(Collection<X509Certificate2>);
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", certs.GetRawCertData()), "dn");

            //assert
            value.CastTo<Collection<X509Certificate2>>().Should().Contain(certs);
        }

        [TestMethod]
        public void FormatValueFromDirectory_SingleX509Certificate_ReturnsAsX509Certificate2Collection()
        {
            //prepare
            var certs = new X509Certificate(Resources.cert);
            _mappingArguments.PropertyType = typeof(Collection<X509Certificate>);
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", certs.GetRawCertData()), "dn");

            //assert
            value.CastTo<Collection<X509Certificate>>().Should().Contain(certs);
        }

        [TestMethod]
        public void IsEqual_SameLengthDifferentCollections_ReturnsFalse()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<X509Certificate2>);
            _mappingArguments.Getter = t => new Collection<X509Certificate2>(new[] { new X509Certificate2(Resources.cert), new X509Certificate2(Resources.cert) });
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<X509Certificate2>(new[] { new X509Certificate2(Resources.cert), new X509Certificate2(Resources.cert2) }), out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_DifferentLengths_ReturnsFalse()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<X509Certificate2>);
            _mappingArguments.Getter = t => new Collection<X509Certificate2>(new[] { new X509Certificate2(Resources.cert), new X509Certificate2(Resources.cert) });
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<X509Certificate2>(new[] { new X509Certificate2(Resources.cert) }), out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<X509Certificate2>);
            _mappingArguments.Getter = t => null;
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<X509Certificate2>(new[] { new X509Certificate2(Resources.cert) }), out modification);

            //assert
            value.Should().BeFalse();
            modification.Should().NotBeNull();
        }

        [TestMethod]
        public void IsEqual_SameArrays_ReturnsTrue()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(Collection<X509Certificate>);
            _mappingArguments.Getter = t => new Collection<X509Certificate>(new[] { new X509Certificate2(Resources.cert), new X509Certificate2(Resources.cert2) });
            var propertyMapping = new X509Certificate2CollectionPropertyMapping<X509Certificate2CollectionPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new Collection<X509Certificate>(new[] { new X509Certificate2(Resources.cert), new X509Certificate2(Resources.cert2) }), out modification);

            //assert
            value.Should().BeTrue();
            modification.Should().BeNull();
        }
    }
}