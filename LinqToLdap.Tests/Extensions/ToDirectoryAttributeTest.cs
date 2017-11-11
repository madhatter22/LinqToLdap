using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Extensions
{
    [TestClass]
    public class ToDirectoryAttributeTest
    {
        [TestMethod]
        public void ToDirectoryAttribute_Null_ReturnsAttributeWithoutValiue()
        {
            object o = null;

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(0);
        }

        [TestMethod]
        public void ToDirectoryAttribute_StringArray_ReturnsCorrectAttribute()
        {
            object o = new []{"one", "two"};

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(2);
            attribute[0].Should().Be.EqualTo("one");
            attribute[1].Should().Be.EqualTo("two");
        }

        [TestMethod]
        public void ToDirectoryAttribute_StringList_ReturnsCorrectAttribute()
        {
            object o = new List<string>{"one", "two"};

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(2);
            attribute[0].Should().Be.EqualTo("one");
            attribute[1].Should().Be.EqualTo("two");
        }

        [TestMethod]
        public void ToDirectoryAttribute_ByteArray_ReturnsCorrectAttribute()
        {
            object o = new byte[] {1, 2};

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(1);
            attribute[0].Should().Be.EqualTo(o);
        }

        [TestMethod]
        public void ToDirectoryAttribute_ListBytes_ReturnsCorrectAttribute()
        {
            object o = new List<byte> { 1, 2 };

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(1);
            attribute[0].As<byte[]>().Should().Have.SameSequenceAs(o as IEnumerable<byte>);
        }

        [TestMethod]
        public void ToDirectoryAttribute_Guid_ReturnsCorrectAttribute()
        {
            object o = Guid.NewGuid();

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(1);
            attribute[0].As<byte[]>().Should().Have.SameSequenceAs(o.CastTo<Guid>().ToByteArray());
        }

        [TestMethod]
        public void ToDirectoryAttribute_SecurityIdentifier_ReturnsCorrectAttribute()
        {
            var bytes = new byte[]
                            {
                                1, 5, 0, 0, 27, 14, 3, 139, 251, 73, 97, 48, 157, 6, 235, 192, 201,
                                125, 33, 65, 182, 209, 6, 82, 206, 165, 32, 24
                            };
            object o = new SecurityIdentifier(bytes, 0);

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(1);
            attribute[0].As<byte[]>().Should().Have.SameSequenceAs(bytes);
        }

        [TestMethod]
        public void ToDirectoryAttribute_ListByteArrays_ReturnsCorrectAttribute()
        {
            var bytes1 = new byte[] {1, 2};
            var bytes2 = new byte[] { 3, 4 };
            object o = new[] {bytes1, bytes2};

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(2);
            attribute[0].Should().Be.EqualTo(bytes1);
            attribute[1].Should().Be.EqualTo(bytes2);
        }

        [TestMethod]
        public void ToDirectoryAttribute_ArrayInts_ReturnsCorrectAttribute()
        {
            object o = new[] { 1, 2 };

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(2);
            attribute[0].Should().Be.EqualTo("1");
            attribute[1].Should().Be.EqualTo("2");
        }

        [TestMethod]
        public void ToDirectoryAttribute_ListInts_ReturnsCorrectAttribute()
        {
            object o = new List<int> { 1, 2 };

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(2);
            attribute[0].Should().Be.EqualTo("1");
            attribute[1].Should().Be.EqualTo("2");
        }

        [TestMethod]
        public void ToDirectoryAttribute_String_ReturnsCorrectAttribute()
        {
            object o = "test";

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(1);
            attribute[0].Should().Be.EqualTo("test");
        }

        [TestMethod]
        public void ToDirectoryAttribute_True_ReturnsCorrectAttribute()
        {
            bool? o = true;

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(1);
            attribute[0].Should().Be.EqualTo("TRUE");
        }

        [TestMethod]
        public void ToDirectoryAttribute_False_ReturnsCorrectAttribute()
        {
            object o = false;

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(1);
            attribute[0].Should().Be.EqualTo("FALSE");
        }

        [TestMethod]
        public void ToDirectoryAttribute_X509Certificate_ReturnsCorrectAttribute()
        {
            var cert = new X509Certificate(Properties.Resources.cert);

            var attribute = cert.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(1);
            attribute[0].As<byte[]>().Should().Have.SameSequenceAs(cert.GetRawCertData());
        }

        [TestMethod]
        public void ToDirectoryAttribute_X509Certificate2_ReturnsCorrectAttribute()
        {
            var cert = new X509Certificate2(Properties.Resources.cert);

            var attribute = cert.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(1);
            attribute[0].As<byte[]>().Should().Have.SameSequenceAs(cert.GetRawCertData());
        }

        [TestMethod]
        public void ToDirectoryAttribute_EnumerableX509Certificate_ReturnsCorrectAttribute()
        {
            var cert = new[] { new X509Certificate(Properties.Resources.cert), new X509Certificate(Properties.Resources.cert) };

            var attribute = cert.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(2);
            attribute[0].As<byte[]>().Should().Have.SameSequenceAs(cert[0].GetRawCertData());
            attribute[1].As<byte[]>().Should().Have.SameSequenceAs(cert[1].GetRawCertData());
        }

        [TestMethod]
        public void ToDirectoryAttribute_EnumerableX509Certificate2_ReturnsCorrectAttribute()
        {
            var cert = new[] { new X509Certificate2(Properties.Resources.cert), new X509Certificate2(Properties.Resources.cert) };

            var attribute = cert.ToDirectoryAttribute("x");

            attribute.Name.Should().Be.EqualTo("x");
            attribute.Count.Should().Be.EqualTo(2);
            attribute[0].As<byte[]>().Should().Have.SameSequenceAs(cert[0].GetRawCertData());
            attribute[1].As<byte[]>().Should().Have.SameSequenceAs(cert[1].GetRawCertData());
        }
    }
}
