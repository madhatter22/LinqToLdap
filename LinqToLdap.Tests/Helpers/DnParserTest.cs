using System;
using LinqToLdap.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Helpers
{
    [TestClass]
    public class DnParserTest
    {
        [TestMethod]
        public void GetEntryName_SingleCharacter_ReturnsCN()
        {
            DnParser.GetEntryName("n=test,Dd=domain,Dd=com").Should().Be.EqualTo("n=test");
        }

        [TestMethod]
        public void GetEntryName_CN_ReturnsCN()
        {
            DnParser.GetEntryName("Cn=test,Dd=domain,Dd=com").Should().Be.EqualTo("Cn=test");
        }

        [TestMethod]
        public void GetEntryName_OU_ReturnsOU()
        {
            DnParser.GetEntryName("OU=test,Cn=test2,Dc=domain,Dc=com").Should().Be.EqualTo("OU=test");
        }

        [TestMethod]
        public void GetEntryName_RDNWithComma_ReturnsOU()
        {
            DnParser.GetEntryName("OU=Doe, John,Cn=test2,Dc=domain,Dc=com").Should().Be.EqualTo("OU=Doe, John");
        }

        [TestMethod]
        public void GetEntryName_OneRDN_ReturnsOU()
        {
            DnParser.GetEntryName("OU=DoeJohn").Should().Be.EqualTo("OU=DoeJohn");
        }

        [TestMethod]
        public void GetEntryContainer_RDNWithComma_ReturnsOU()
        {
            DnParser.GetEntryContainer("OU=Doe, John,Cn=test2,Dc=domain,Dc=com").Should().Be.EqualTo("Cn=test2,Dc=domain,Dc=com");
        }

        [TestMethod]
        public void GetEntryContainer_OneRDN_ReturnsOU()
        {
            DnParser.GetEntryContainer("OU=DoeJohn").Should().Be.EqualTo("OU=DoeJohn");
        }

        [TestMethod]
        public void GetEntryContainer_CN_ReturnsContainer()
        {
            DnParser.GetEntryContainer("Cn=test,Dc=domain,Dc=com").Should().Be.EqualTo("Dc=domain,Dc=com");
        }

        [TestMethod]
        public void GetEntryContainer_OU_ReturnsContainer()
        {
            DnParser.GetEntryContainer("OU=test,Cn=test2,Dc=domain,Dc=com").Should().Be.EqualTo("Cn=test2,Dc=domain,Dc=com");
        }

        [TestMethod]
        public void ParseRDN_SingleCharacter_ReturnsPrefix()
        {
            DnParser.ParseRDN("n=test,Dd=domain,Dd=com").Should().Be.EqualTo("n");
        }

        [TestMethod]
        public void ParseRDN_MultipleCharacters_ReturnsPrefix()
        {
            DnParser.ParseRDN("XYZ=test,Dd=domain,Dd=com").Should().Be.EqualTo("XYZ");
        }

        [TestMethod]
        public void ParseRDN_NullDn_ThrowsException()
        {
            Executing.This(() => DnParser.ParseRDN(null))
                .Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ParseRDN_BadEqualsIndex_ThrowsException()
        {
            Executing.This(() => DnParser.ParseName("=test,Cn=test2,Dc=domain,Dc=com"))
                .Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ParseRDN_NoEqualsIndex_ThrowsException()
        {
            Executing.This(() => DnParser.ParseName("Test"))
                .Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ParseName_SingleCharacter_ReturnsCN()
        {
            DnParser.ParseName("n=test,Dd=domain,Dd=com").Should().Be.EqualTo("test");
        }

        [TestMethod]
        public void ParseName_OneRDN_ReturnsOU()
        {
            DnParser.ParseName("OU=DoeJohn").Should().Be.EqualTo("DoeJohn");
        }

        [TestMethod]
        public void ParseName_CN_ReturnsCN()
        {
            DnParser.ParseName("Cn=test,Dc=domain,Dc=com").Should().Be.EqualTo("test");
        }

        [TestMethod]
        public void ParseName_OU_ReturnsOU()
        {
            DnParser.ParseName("OU=test,Cn=test2,Dc=domain,Dc=com").Should().Be.EqualTo("test");
        }

        [TestMethod]
        public void ParseName_NullDn_ThrowsException()
        {
            Executing.This(() => DnParser.ParseName(null))
                .Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ParseName_BadEqualsIndex_ThrowsException()
        {
            Executing.This(() => DnParser.ParseName("=test,Cn=test2,Dc=domain,Dc=com"))
                .Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void ParseName_NoEqualsIndex_ThrowsException()
        {
            Executing.This(() => DnParser.ParseName("Test"))
                .Should().Throw<ArgumentException>();
        }

        [TestMethod]
        public void FormatName_NoPrefix_AppendsPrefix()
        {
            var formatted = DnParser.FormatName("t", "Cn=test,Dc=domain,Dc=com");

            formatted.Should().Be.EqualTo("Cn=t");
        }

        [TestMethod]
        public void FormatName_WithPrefix_DoesNotAppendsPrefix()
        {
            var formatted = DnParser.FormatName("CN=t", "Cn=test,Dc=domain,Dc=com");

            formatted.Should().Be.EqualTo("CN=t");
        }
    }
}
