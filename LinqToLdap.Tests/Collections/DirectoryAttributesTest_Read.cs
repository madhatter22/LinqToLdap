using System;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using LinqToLdap.Collections;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Collections
{
    [TestClass]
    public class DirectoryAttributesTest
    {
        private SearchResultAttributeCollection _collection;
        private SearchResultEntry _entry;
        private IDirectoryAttributes _attributes;
        private byte[] _guidBytes;
        private byte[] _siBytes;
        private byte[][] _byteArrays;

        [TestInitialize]
        public void SetUp()
        {
            _guidBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            _siBytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28 };
            var strings = new[] { "one", "two", "three", "four" };
            _byteArrays = new[] { _guidBytes, _siBytes };

            _collection =
                typeof(SearchResultAttributeCollection).Create<SearchResultAttributeCollection>();
            _collection
                .Call("Add", new object[] { "Property1", new DirectoryAttribute("Property1", "prop1") });
            _collection
                .Call("Add", new object[] { "Property2", new DirectoryAttribute("Property2", "2") });
            _collection
                .Call("Add", new object[] { "Property3", new DirectoryAttribute("Property3", _guidBytes) });
            _collection
                .Call("Add", new object[] { "Property4", new DirectoryAttribute("Property4", strings) });
            _collection
                .Call("Add", new object[] { "Property5", new DirectoryAttribute("Property5", _siBytes) });
            _collection
                .Call("Add", new object[] { "Property6", new DirectoryAttribute("Property6", "TRUE") });
            _collection
                .Call("Add", new object[] { "Property7", new DirectoryAttribute("Property7", "20110313064859.0Z") });
            _collection
                .Call("Add", new object[] { "Property8", new DirectoryAttribute("Property8", "129444725394225946") });
            _collection
                .Call("Add", new object[] { "Property9", new DirectoryAttribute("Property9", "20110313064859Z") });
            _collection
                .Call("Add", new object[] { "Property10", new DirectoryAttribute("Property10", _byteArrays) });
            _collection
                .Call("Add", new object[] { "Property11", new DirectoryAttribute("Property10", Properties.Resources.cert) });

            _entry =
                typeof(SearchResultEntry).Create<SearchResultEntry>(
                    new object[] { "theDn", _collection });

            _attributes = new DirectoryAttributes(_entry);
        }

        [TestMethod]
        public void GetValue_ExistsAndCorrectType_ReturnsByte()
        {
            _attributes.GetValue("property2").Should().Be.EqualTo("2");
        }

        [TestMethod]
        public void Indexer_ExistsAndCorrectType_ReturnsByte()
        {
            _attributes["property2"].Should().Be.EqualTo("2");
        }

        [TestMethod]
        public void GetByte_ExistsAndCorrectType_ReturnsByte()
        {
            _attributes.GetByte("property2").Should().Be.EqualTo(2);
        }

        [TestMethod]
        public void GetByte_DoesNotExistAndCorrectType_ReturnsNull()
        {
            _attributes.GetByte("nope").Should().Not.Have.Value();
        }

        [TestMethod]
        public void GetByte_ExistsAndWrongType_ThrowsFormatException()
        {
            Executing.This(() => _attributes.GetByte("property1"))
                .Should().Throw<FormatException>()
                .And.Exception.Satisfies(e => e.Message.Equals("Value 'prop1' for attribute 'property1' caused FormatException when trying to convert to 'Byte' for theDn") &&
                    e.InnerException.As<FormatException>() != null);
        }

        [TestMethod]
        public void GetBoolean_ExistsAndCorrectType_ReturnsBoolean()
        {
            _attributes.GetBoolean("property6").Should().Be.EqualTo(true);
        }

        [TestMethod]
        public void GetBoolean_DoesNotExistAndCorrectType_ReturnsNull()
        {
            _attributes.GetBoolean("nope").Should().Not.Have.Value();
        }

        [TestMethod]
        public void GetBoolean_ExistsAndWrongType_ThrowsFormatException()
        {
            Executing.This(() => _attributes.GetBoolean("property1"))
                .Should().Throw<FormatException>()
                .Exception.Satisfies(e => e.Message.Equals("Value 'prop1' for attribute 'property1' caused FormatException when trying to convert to 'Boolean' for theDn") &&
                        e.InnerException.GetType().Equals(typeof(FormatException)));
        }

        [TestMethod]
        public void GetShort_ExistsAndCorrectType_ReturnsShort()
        {
            _attributes.GetShort("property2").Should().Be.EqualTo(2);
        }

        [TestMethod]
        public void GetShort_DoesNotExistAndCorrectType_ReturnsNull()
        {
            _attributes.GetShort("nope").Should().Not.Have.Value();
        }

        [TestMethod]
        public void GetShort_ExistsAndWrongType_ThrowsFormatException()
        {
            Executing.This(() => _attributes.GetShort("property1"))
                .Should().Throw<FormatException>().And.Exception
                .Satisfies(e => e.Message.Equals("Value 'prop1' for attribute 'property1' caused FormatException when trying to convert to 'Int16' for theDn") &&
                        e.InnerException.As<FormatException>() != null);
        }

        [TestMethod]
        public void GetInt_ExistsAndCorrectType_ReturnsInt()
        {
            _attributes.GetInt("property2").Should().Be.EqualTo(2);
        }

        [TestMethod]
        public void GetInt_DoesNotExistAndCorrectType_ReturnsNull()
        {
            _attributes.GetInt("nope").Should().Not.Have.Value();
        }

        [TestMethod]
        public void GetInt_ExistsAndWrongType_ThrowsFormatException()
        {
            Executing.This(() => _attributes.GetInt("property1"))
                .Should().Throw<FormatException>().And.Exception
                .Satisfies(e => e.Message.Equals("Value 'prop1' for attribute 'property1' caused FormatException when trying to convert to 'Int32' for theDn") &&
                        e.InnerException.As<FormatException>() != null);
        }

        [TestMethod]
        public void GetLong_ExistsAndCorrectType_ReturnsLong()
        {
            _attributes.GetLong("property2").Should().Be.EqualTo(2);
        }

        [TestMethod]
        public void GetLong_DoesNotExistAndCorrectType_ReturnsNull()
        {
            _attributes.GetLong("nope").Should().Not.Have.Value();
        }

        [TestMethod]
        public void GetLong_ExistsAndWrongType_ThrowsFormatException()
        {
            Executing.This(() => _attributes.GetLong("property1"))
                .Should().Throw<FormatException>().And.Exception
                .Satisfies(e => e.Message.Equals("Value 'prop1' for attribute 'property1' caused FormatException when trying to convert to 'Int64' for theDn") &&
                        e.InnerException.As<FormatException>() != null);
        }

        [TestMethod]
        public void GetFloat_ExistsAndCorrectType_ReturnsFloat()
        {
            _attributes.GetFloat("property2").Should().Be.EqualTo(2);
        }

        [TestMethod]
        public void GetFloat_DoesNotExistAndCorrectType_ReturnsNull()
        {
            _attributes.GetFloat("nope").Should().Not.Have.Value();
        }

        [TestMethod]
        public void GetFloat_ExistsAndWrongType_ThrowsFormatException()
        {
            Executing.This(() => _attributes.GetFloat("property1"))
                .Should().Throw<FormatException>().And.Exception
                .Satisfies(e => e.Message.Equals("Value 'prop1' for attribute 'property1' caused FormatException when trying to convert to 'Single' for theDn") &&
                        e.InnerException.As<FormatException>() != null);
        }

        [TestMethod]
        public void GetDouble_ExistsAndCorrectType_ReturnsDouble()
        {
            _attributes.GetDouble("property2").Should().Be.EqualTo(2);
        }

        [TestMethod]
        public void GetDouble_DoesNotExistAndCorrectType_ReturnsNull()
        {
            _attributes.GetDouble("nope").Should().Not.Have.Value();
        }

        [TestMethod]
        public void GetDouble_ExistsAndWrongType_ThrowsFormatException()
        {
            Executing.This(() => _attributes.GetDouble("property1"))
                .Should().Throw<FormatException>().And.Exception
                .Satisfies(e => e.Message.Equals("Value 'prop1' for attribute 'property1' caused FormatException when trying to convert to 'Double' for theDn") &&
                        e.InnerException.As<FormatException>() != null);
        }

        [TestMethod]
        public void GetDecimal_ExistsAndCorrectType_ReturnsDecimal()
        {
            _attributes.GetDecimal("property2").Should().Be.EqualTo(2);
        }

        [TestMethod]
        public void GetDecimal_DoesNotExistAndCorrectType_ReturnsNull()
        {
            _attributes.GetDecimal("nope").Should().Not.Have.Value();
        }

        [TestMethod]
        public void GetDecimal_ExistsAndWrongType_ThrowsFormatException()
        {
            Executing.This(() => _attributes.GetDecimal("property1"))
                .Should().Throw<FormatException>().And.Exception
                .Satisfies(e => e.Message.Equals("Value 'prop1' for attribute 'property1' caused FormatException when trying to convert to 'Decimal' for theDn") &&
                        e.InnerException.As<FormatException>() != null);
        }

        [TestMethod]
        public void GetDateTime_ExistsAndCorrectTypeWithDefaultFormat_ReturnsDateTime()
        {
            _attributes.GetDateTime("property7").Should().Be.EqualTo("20110313064859.0Z".FormatLdapDateTime(ExtensionMethods.LdapFormat));
        }

        [TestMethod]
        public void GetDateTime_DoesNotExistAndCorrectType_ReturnsNull()
        {
            _attributes.GetDateTime("nope").Should().Not.Have.Value();
        }

        [TestMethod]
        public void GetDateTime_ExistsAndCorrectTypeWithDifferentFormat_ReturnsDateTime()
        {
            var dateTime = "20110313064859Z".FormatLdapDateTime("yyyyMMddHHmmssZ");
            _attributes.GetDateTime("property9", "yyyyMMddHHmmssZ").Should().Be.EqualTo(dateTime);
        }

        [TestMethod]
        public void GetDateTime_ExistsAndCorrectTypeWithFileTimeFormat_ReturnsDateTime()
        {
            _attributes.GetDateTime("property8", null).Should().Be.EqualTo(DateTime.FromFileTime(129444725394225946));
        }

        [TestMethod]
        public void GetDateTime_ExistsAndWrongTypeWithWrongFormat_ThrowsFormatException()
        {
            Executing.This(() => _attributes.GetDateTime("property1"))
                .Should().Throw<FormatException>().And.Exception
                .Satisfies(e => e.Message.Equals("Value 'prop1' for attribute 'property1' caused FormatException when trying to convert to 'DateTime' for theDn") &&
                        e.InnerException.As<FormatException>() != null);
        }

        [TestMethod]
        public void GetDateTime_ExistsAndCorrectTypeWithWrongFormat_ThrowsFormatException()
        {
            Executing.This(() => _attributes.GetDateTime("property9"))
                .Should().Throw<FormatException>().And.Exception
                .Satisfies(e => e.Message.Equals("Value '20110313064859Z' for attribute 'property9' caused FormatException when trying to convert to 'DateTime' for theDn") &&
                        e.InnerException.As<FormatException>() != null);
        }

        [TestMethod]
        public void GetString_Exists_ReturnsString()
        {
            _attributes.GetString("property1").Should().Be.EqualTo("prop1");
        }

        [TestMethod]
        public void GetString_DoesNotExist_ReturnsNull()
        {
            _attributes.GetString("nope").Should().Be.Null();
        }

        [TestMethod]
        public void GetStrings_Exists_ReturnsStringArray()
        {
            _attributes.GetStrings("property4").Should().Have.SameSequenceAs(new[] { "one", "two", "three", "four" });
        }

        [TestMethod]
        public void GetStrings_ExistsAsSingleValue_ReturnsStringArray()
        {
            _attributes.GetStrings("property1").Should().Have.SameSequenceAs(new[] { "prop1" });
        }

        [TestMethod]
        public void GetStrings_DoesNotExist_ReturnsEmpty()
        {
            _attributes.GetStrings("nope").Should().Be.Empty();
        }

        [TestMethod]
        public void GetBytes_ExistsAndCorrectType_ReturnsByteArray()
        {
            _attributes.GetBytes("property3").Should().Have.SameSequenceAs(_guidBytes);
        }

        [TestMethod]
        public void GetBytes_DoesNotExistAndCorrectType_ReturnsNull()
        {
            _attributes.GetBytes("nope").Should().Be.Null();
        }

        [TestMethod]
        public void GetGuid_ExistsAndCorrectType_ReturnsGuid()
        {
            _attributes.GetGuid("property3").Should().Be.EqualTo(new Guid(_guidBytes));
        }

        [TestMethod]
        public void GetGuid_DoesNotExistAndCorrectType_ReturnsNull()
        {
            _attributes.GetGuid("nope").Should().Not.Have.Value();
        }

        [TestMethod]
        public void GetGuid_ExistsAndWrongType_ThrowsFormatException()
        {
#if NET35
            var str = string.Join(",", System.Text.Encoding.ASCII.GetBytes("prop1").Select(b => b.ToString(CultureInfo.InvariantCulture)).ToArray());
#else
            var str = string.Join(",", System.Text.Encoding.ASCII.GetBytes("prop1"));
#endif
            Executing.This(() => _attributes.GetGuid("property1"))
                .Should().Throw<FormatException>().And.Exception
                .Satisfies(e => e.Message.Equals("Value '" + str + "' for attribute 'property1' caused ArgumentException when trying to convert to 'Guid' for theDn") &&
                        e.InnerException.As<ArgumentException>() != null);
        }

        [TestMethod]
        public void GetSecurityIdentifier_ExistsAndCorrectType_ReturnsSecurityIdentifier()
        {
            _attributes.GetSecurityIdentifier("property5").Should().Be.EqualTo(new SecurityIdentifier(_siBytes, 0));
        }

        [TestMethod]
        public void GetSecurityIdentifier_DoesNotExistAndCorrectType_ReturnsNull()
        {
            _attributes.GetSecurityIdentifier("nope").Should().Be.Null();
        }

        [TestMethod]
        public void GetSecurityIdentifier_ExistsAndWrongType_ThrowsFormatException()
        {
#if NET35
            var str = string.Join(",", System.Text.Encoding.ASCII.GetBytes("prop1").Select(b => b.ToString(CultureInfo.InvariantCulture)).ToArray());
#else
            var str = string.Join(",", System.Text.Encoding.ASCII.GetBytes("prop1"));
#endif
            Executing.This(() => _attributes.GetSecurityIdentifier("property1"))
                .Should().Throw<FormatException>().And.Exception
                .Satisfies(e => e.Message.Equals("Value '" + str + "' for attribute 'property1' caused ArgumentOutOfRangeException when trying to convert to 'SecurityIdentifier' for theDn") &&
                        e.InnerException.As<ArgumentException>() != null);
        }

        [TestMethod]
        public void GetByteArrays_ExistsAndCorrectType_ReturnsListOfByteArrays()
        {
            _attributes.GetByteArrays("property10").Should().Have.SameSequenceAs(_byteArrays);
        }

        [TestMethod]
        public void GetByteArrays_DoesNotExist_ReturnsEmpty()
        {
            _attributes.GetByteArrays("nope").Should().Be.Empty();
        }

        [TestMethod]
        public void GetByteArrays_ExistsAndSingleValueCorrectType_ReturnsListOfByteArrays()
        {
            _attributes.GetByteArrays("property3").Should().Contain(_guidBytes).And.Have.Count.EqualTo(1);
        }

        [TestMethod]
        public void GetX509Certificate_ExistsAndCorrectType_ReturnsX509Certificate()
        {
            _attributes.GetX509Certificate("property11").Should().Be.EqualTo(new X509Certificate(Properties.Resources.cert));
        }

        [TestMethod]
        public void GetX509Certificate_DoesNotExist_ReturnsNull()
        {
            _attributes.GetX509Certificate("nope").Should().Be.Null();
        }

        [TestMethod]
        public void GetX509Certificate2_ExistsAndCorrectType_ReturnsX509Certificate2()
        {
            _attributes.GetX509Certificate2("property11").Should().Be.EqualTo(new X509Certificate2(Properties.Resources.cert));
        }

        [TestMethod]
        public void GetX509Certificate2_DoesNotExist_ReturnsNull()
        {
            _attributes.GetX509Certificate2("nope").Should().Be.Null();
        }

        [TestMethod]
        public void GetX509Certificates_ExistsAndCorrectType_ReturnsListOfX509Certificates()
        {
            _attributes.GetX509Certificates("property11").Should().Have.SameSequenceAs(new[] { new X509Certificate(Properties.Resources.cert) });
        }

        [TestMethod]
        public void GetX509Certificates_DoesNotExist_ReturnsNull()
        {
            _attributes.GetX509Certificates("nope").Should().Be.Null();
        }

        [TestMethod]
        public void GetX509Certificate2s_ExistsAndCorrectType_ReturnsListOfX509Certificates()
        {
            _attributes.GetX509Certificate2s("property11").Should().Have.SameSequenceAs(new[] { new X509Certificate2(Properties.Resources.cert) });
        }

        [TestMethod]
        public void GetX509Certificate2s_DoesNotExist_ReturnsNull()
        {
            _attributes.GetX509Certificate2s("nope").Should().Be.Null();
        }

        [TestMethod]
        public void AddModification_NullModification_ThrowsArgumentNullException()
        {
            Executing.This(() => _attributes.AddModification(null))
                     .Should().Throw<ArgumentNullException>().And.Exception.ParamName
                     .Should().Be.EqualTo("modification");
        }

        [TestMethod]
        public void AddModification_NullModification_ThrowsArgumentException()
        {
            Executing.This(() => _attributes.AddModification(new DirectoryAttributeModification()))
                     .Should().Throw<ArgumentException>().And.Exception.Message
                     .Should().Be.EqualTo("The modification must have a name.");
        }

        [TestMethod]
        public void AddModification_DistinguishedName_ThrowsArgumentException()
        {
            Executing.This(() => _attributes.AddModification(new DirectoryAttributeModification { Name = "Distinguishedname" }))
                     .Should().Throw<ArgumentException>().And.Exception.Message
                     .Should().Be.EqualTo("Cannot change the distinguished name. Please use MoveEntry or RenameEntry.");
        }

        [TestMethod]
        public void AddModification_EntryDN_ThrowsArgumentException()
        {
            Executing.This(() => _attributes.AddModification(new DirectoryAttributeModification { Name = "Entrydn" }))
                     .Should().Throw<ArgumentException>().And.Exception.Message
                     .Should().Be.EqualTo("Cannot change the distinguished name. Please use MoveEntry or RenameEntry.");
        }

        [TestMethod]
        public void AddModification_Modification_Adds()
        {
            //prepare
            var mod = new DirectoryAttributeModification { Name = "Test" };

            //act
            _attributes.AddModification(mod);

            //assert
            _attributes.GetChangedAttributes().Should().Contain(mod);
        }

        [TestMethod]
        public void AddModification_TwoModificationsWithDifferentOperations_Adds()
        {
            //prepare
            var mod1 = new DirectoryAttributeModification { Name = "Test", Operation = DirectoryAttributeOperation.Delete };
            var mod2 = new DirectoryAttributeModification { Name = "Test", Operation = DirectoryAttributeOperation.Add };

            //act
            _attributes.AddModification(mod1);
            _attributes.AddModification(mod2);

            //assert
            _attributes.GetChangedAttributes().Should().Contain(mod1).And.Contain(mod2);
        }

        [TestMethod]
        public void AddModification_TwoModificationsWithSameOperations_ThrowsInvalidOperationException()
        {
            //prepare
            var mod1 = new DirectoryAttributeModification { Name = "Test", Operation = DirectoryAttributeOperation.Delete };
            var mod2 = new DirectoryAttributeModification { Name = "Test", Operation = DirectoryAttributeOperation.Delete };
            _attributes.AddModification(mod1);

            //act
            Executing.This(() => _attributes.AddModification(mod2))
                .Should().Throw<InvalidOperationException>().And.Exception.Message
                .Should().Be.EqualTo("A modification for Test with operation Delete has already been added.");
        }
    }
}