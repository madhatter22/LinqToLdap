using System;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using LinqToLdap.Mapping.PropertyMappings;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class X509Certificate2ArrayPropertyMappingTest
    {
        private PropertyMappingArguments<X509Certificate2ArrayPropertyMappingTest> _mappingArguments;

        [TestInitialize]
        public void SetUp()
        {
            _mappingArguments = new PropertyMappingArguments<X509Certificate2ArrayPropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_X509Certificate2Array_ThrowsNotSupportedException()
        {
            //prepare
            var certs = new[] { new X509Certificate2(Properties.Resources.cert), new X509Certificate2(Properties.Resources.cert) };
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => propertyMapping.FormatValueToFilter(certs))
                .Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void FormatValueToFilter_X509CertificateArray_ThrowsNotSupportedException()
        {
            //prepare
            var certs = new[] { new X509Certificate(Properties.Resources.cert), new X509Certificate(Properties.Resources.cert) };
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);

            //act
            Executing.This(() => propertyMapping.FormatValueToFilter(certs))
                .Should().Throw<NotSupportedException>();
        }

        [TestMethod]
        public void FormatValueToFilter_X509Certificate2_ReturnsStringOctet()
        {
            //prepare
            var certs = new X509Certificate2(Properties.Resources.cert);
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueToFilter(certs);

            //assert
            value.Should().Be.EqualTo(certs.GetRawCertData().ToStringOctet());
        }

        [TestMethod]
        public void FormatValueToFilter_X509Certificate_ReturnsStringOctet()
        {
            //prepare
            var certs = new X509Certificate(Properties.Resources.cert);
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueToFilter(certs);

            //assert
            value.Should().Be.EqualTo(certs.GetRawCertData().ToStringOctet());
        }

        [TestMethod]
        public void FormatValueFromDirectory_X509Certificate2Array_ReturnsX509Certificate2Array()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(X509Certificate2[]);
            var certs = new[] { new X509Certificate2(Properties.Resources.cert), new X509Certificate2(Properties.Resources.cert) };
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", certs.Select(c => c.GetRawCertData()).ToArray()), "dn");

            //assert
            value.As<X509Certificate2[]>().Should().Have.SameSequenceAs(certs);
        }

        [TestMethod]
        public void FormatValueFromDirectory_X509CertificateArray_ReturnsX509CertificateArray()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(X509Certificate[]);
            var certs = new[] { new X509Certificate(Properties.Resources.cert), new X509Certificate(Properties.Resources.cert) };
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", certs.Select(c => c.GetRawCertData()).ToArray()), "dn");

            //assert
            value.As<X509Certificate[]>().Should().Have.SameSequenceAs(certs);
        }

        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(X509Certificate2[]);
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void FormatValueFromDirectory_SingleX509Certificate2_ReturnsAsX509Certificate2Array()
        {
            //prepare
            var certs = new X509Certificate2(Properties.Resources.cert);
            _mappingArguments.PropertyType = typeof(X509Certificate2[]);
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", certs.GetRawCertData()), "dn");

            //assert
            value.As<X509Certificate2[]>().Should().Contain(certs);
        }

        [TestMethod]
        public void FormatValueFromDirectory_SingleX509Certificate_ReturnsAsX509Certificate2Array()
        {
            //prepare
            var certs = new X509Certificate(Properties.Resources.cert);
            _mappingArguments.PropertyType = typeof(X509Certificate[]);
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", certs.GetRawCertData()), "dn");

            //assert
            value.As<X509Certificate[]>().Should().Contain(certs);
        }

        [TestMethod]
        public void IsEqual_SameLengthDifferentArrays_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new[] { new X509Certificate2(Properties.Resources.cert), new X509Certificate2(Properties.Resources.cert) };
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new[] { new X509Certificate2(Properties.Resources.cert), new X509Certificate2(Properties.Resources.cert2) }, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_DifferentLengths_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => new[] { new X509Certificate2(Properties.Resources.cert), new X509Certificate2(Properties.Resources.cert) };
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new[] { new X509Certificate2(Properties.Resources.cert) }, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new[] { new X509Certificate2(Properties.Resources.cert) }, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_SameArrays_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => new[] { new X509Certificate2(Properties.Resources.cert), new X509Certificate2(Properties.Resources.cert2) };
            var propertyMapping = new X509Certificate2ArrayPropertyMapping<X509Certificate2ArrayPropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, new[] { new X509Certificate2(Properties.Resources.cert), new X509Certificate2(Properties.Resources.cert2) }, out modification);

            //assert
            value.Should().Be.True();
            modification.Should().Be.Null();
        }
    }
}
