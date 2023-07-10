﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
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

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(0);
        }

        [TestMethod]
        public void ToDirectoryAttribute_StringArray_ReturnsCorrectAttribute()
        {
            object o = new[] { "one", "two" };

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(2);
            attribute[0].Should().Be("one");
            attribute[1].Should().Be("two");
        }

        [TestMethod]
        public void ToDirectoryAttribute_StringList_ReturnsCorrectAttribute()
        {
            object o = new List<string> { "one", "two" };

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(2);
            attribute[0].Should().Be("one");
            attribute[1].Should().Be("two");
        }

        [TestMethod]
        public void ToDirectoryAttribute_ByteArray_ReturnsCorrectAttribute()
        {
            object o = new byte[] { 1, 2 };

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(1);
            attribute[0].Should().Be(o);
        }

        [TestMethod]
        public void ToDirectoryAttribute_ListBytes_ReturnsCorrectAttribute()
        {
            object o = new List<byte> { 1, 2 };

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(1);
            attribute[0].CastTo<byte[]>().Should().ContainInOrder(o as IEnumerable<byte>);
        }

        [TestMethod]
        public void ToDirectoryAttribute_Guid_ReturnsCorrectAttribute()
        {
            object o = Guid.NewGuid();

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(1);
            attribute[0].CastTo<byte[]>().Should().ContainInOrder(o.CastTo<Guid>().ToByteArray());
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

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(1);
            attribute[0].CastTo<byte[]>().Should().ContainInOrder(bytes);
        }

        [TestMethod]
        public void ToDirectoryAttribute_ListByteArrays_ReturnsCorrectAttribute()
        {
            var bytes1 = new byte[] { 1, 2 };
            var bytes2 = new byte[] { 3, 4 };
            object o = new[] { bytes1, bytes2 };

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(2);
            attribute[0].Should().Be(bytes1);
            attribute[1].Should().Be(bytes2);
        }

        [TestMethod]
        public void ToDirectoryAttribute_ArrayInts_ReturnsCorrectAttribute()
        {
            object o = new[] { 1, 2 };

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(2);
            attribute[0].Should().Be("1");
            attribute[1].Should().Be("2");
        }

        [TestMethod]
        public void ToDirectoryAttribute_ListInts_ReturnsCorrectAttribute()
        {
            object o = new List<int> { 1, 2 };

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(2);
            attribute[0].Should().Be("1");
            attribute[1].Should().Be("2");
        }

        [TestMethod]
        public void ToDirectoryAttribute_String_ReturnsCorrectAttribute()
        {
            object o = "test";

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(1);
            attribute[0].Should().Be("test");
        }

        [TestMethod]
        public void ToDirectoryAttribute_True_ReturnsCorrectAttribute()
        {
            bool? o = true;

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(1);
            attribute[0].Should().Be("TRUE");
        }

        [TestMethod]
        public void ToDirectoryAttribute_False_ReturnsCorrectAttribute()
        {
            object o = false;

            var attribute = o.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(1);
            attribute[0].Should().Be("FALSE");
        }

        [TestMethod]
        public void ToDirectoryAttribute_X509Certificate_ReturnsCorrectAttribute()
        {
            var cert = new X509Certificate(Resources.cert);

            var attribute = cert.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(1);
            attribute[0].CastTo<byte[]>().Should().ContainInOrder(cert.GetRawCertData());
        }

        [TestMethod]
        public void ToDirectoryAttribute_X509Certificate2_ReturnsCorrectAttribute()
        {
            var cert = new X509Certificate2(Resources.cert);

            var attribute = cert.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(1);
            attribute[0].CastTo<byte[]>().Should().ContainInOrder(cert.GetRawCertData());
        }

        [TestMethod]
        public void ToDirectoryAttribute_EnumerableX509Certificate_ReturnsCorrectAttribute()
        {
            var cert = new[] { new X509Certificate(Resources.cert), new X509Certificate(Resources.cert) };

            var attribute = cert.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(2);
            attribute[0].CastTo<byte[]>().Should().ContainInOrder(cert[0].GetRawCertData());
            attribute[1].CastTo<byte[]>().Should().ContainInOrder(cert[1].GetRawCertData());
        }

        [TestMethod]
        public void ToDirectoryAttribute_EnumerableX509Certificate2_ReturnsCorrectAttribute()
        {
            var cert = new[] { new X509Certificate2(Resources.cert), new X509Certificate2(Resources.cert) };

            var attribute = cert.ToDirectoryAttribute("x");

            attribute.Name.Should().Be("x");
            attribute.Count.Should().Be(2);
            attribute[0].CastTo<byte[]>().Should().ContainInOrder(cert[0].GetRawCertData());
            attribute[1].CastTo<byte[]>().Should().ContainInOrder(cert[1].GetRawCertData());
        }
    }
}