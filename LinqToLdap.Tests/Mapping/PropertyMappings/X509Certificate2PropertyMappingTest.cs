using System.DirectoryServices.Protocols;
using System.Security.Cryptography.X509Certificates;
using LinqToLdap.Mapping.PropertyMappings;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Mapping.PropertyMappings
{
    [TestClass]
    public class X509Certificate2PropertyMappingTest
    {
        private PropertyMappingArguments<X509Certificate2PropertyMappingTest> _mappingArguments;
        private X509Certificate2 _certificate;
        private X509Certificate2 _certificate2;

        [TestInitialize]
        public void SetUp()
        {
            _certificate = new X509Certificate2(Properties.Resources.cert);
            _certificate2 = new X509Certificate2(Properties.Resources.cert2);
            
            _mappingArguments = new PropertyMappingArguments<X509Certificate2PropertyMappingTest>
            {
                AttributeName = "att",
                PropertyName = "name",
                PropertyType = typeof(object)
            };
        }

        [TestMethod]
        public void FormatValueToFilter_X509Certificate2_ReturnsStringOctet()
        {
            //prepare
            var propertyMapping = new X509Certificate2PropertyMapping<X509Certificate2PropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueToFilter(_certificate);

            //assert
            value.Should().Be.EqualTo(_certificate.GetRawCertData().ToStringOctet());
        }

        [TestMethod]
        public void FormatValueFromDirectory_ByteArray_ReturnsX509Certificate2()
        {
            //prepare
            var propertyMapping = new X509Certificate2PropertyMapping<X509Certificate2PropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(new DirectoryAttribute("name", _certificate.GetRawCertData()), "dn");

            //assert
            value.CastTo<X509Certificate2>().GetRawCertData().Should().Have.SameSequenceAs(_certificate.GetRawCertData());
        }
        
        [TestMethod]
        public void FormatValueFromDirectory_Null_ReturnsNull()
        {
            //prepare
            _mappingArguments.PropertyType = typeof(X509Certificate2);
            var propertyMapping = new X509Certificate2PropertyMapping<X509Certificate2PropertyMappingTest>(_mappingArguments);

            //act
            var value = propertyMapping.FormatValueFromDirectory(null, "dn");

            //assert
            value.Should().Be.Null();
        }

        [TestMethod]
        public void IsEqual_DiffferentCertificates_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => _certificate;
            var propertyMapping = new X509Certificate2PropertyMapping<X509Certificate2PropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;

            //act
            var value = propertyMapping.IsEqual(this, _certificate2, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_OneNull_ReturnsFalse()
        {
            //prepare
            _mappingArguments.Getter = t => null;
            var propertyMapping = new X509Certificate2PropertyMapping<X509Certificate2PropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, _certificate2, out modification);

            //assert
            value.Should().Be.False();
            modification.Should().Not.Be.Null();
        }

        [TestMethod]
        public void IsEqual_SameCertificates_ReturnsTrue()
        {
            //prepare
            _mappingArguments.Getter = t => new X509Certificate2(Properties.Resources.cert);
            var propertyMapping = new X509Certificate2PropertyMapping<X509Certificate2PropertyMappingTest>(_mappingArguments);
            DirectoryAttributeModification modification;
            //act
            var value = propertyMapping.IsEqual(this, _certificate, out modification);

            //assert
            value.Should().Be.True();
            modification.Should().Be.Null();
        }
    }
}
