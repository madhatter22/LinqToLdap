using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
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
    public class ToDirectoryModificationTest
    {
        [TestMethod]
        public void ToDirectoryModification_Null_ReturnsAttributeWithoutValiue()
        {
            object o = null;

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(0);
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_StringArray_ReturnsCorrectAttribute()
        {
            object o = new[] { "one", "two" };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(2);
            modification[0].Should().Be("one");
            modification[1].Should().Be("two");
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_StringList_ReturnsCorrectAttribute()
        {
            object o = new List<string> { "one", "two" };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(2);
            modification[0].Should().Be("one");
            modification[1].Should().Be("two");
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_ByteArray_ReturnsCorrectAttribute()
        {
            object o = new byte[] { 1, 2 };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(1);
            modification[0].Should().Be(o);
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_ListBytes_ReturnsCorrectAttribute()
        {
            object o = new List<byte> { 1, 2 };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(1);
            modification[0].CastTo<byte[]>().Should().ContainInOrder(o as IEnumerable<byte>);
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_Guid_ReturnsCorrectAttribute()
        {
            object o = Guid.NewGuid();

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(1);
            modification[0].CastTo<byte[]>().Should().ContainInOrder(o.CastTo<Guid>().ToByteArray());
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_SecurityIdentifier_ReturnsCorrectAttribute()
        {
            var bytes = new byte[]
                            {
                                1, 5, 0, 0, 27, 14, 3, 139, 251, 73, 97, 48, 157, 6, 235, 192, 201,
                                125, 33, 65, 182, 209, 6, 82, 206, 165, 32, 24
                            };
            object o = new SecurityIdentifier(bytes, 0);

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(1);
            modification[0].CastTo<byte[]>().Should().ContainInOrder(bytes);
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_ListByteArrays_ReturnsCorrectAttribute()
        {
            var bytes1 = new byte[] { 1, 2 };
            var bytes2 = new byte[] { 3, 4 };
            object o = new[] { bytes1, bytes2 };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(2);
            modification[0].Should().Be(bytes1);
            modification[1].Should().Be(bytes2);
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_ArrayInts_ReturnsCorrectAttribute()
        {
            object o = new[] { 1, 2 };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(2);
            modification[0].Should().Be("1");
            modification[1].Should().Be("2");
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_ListInts_ReturnsCorrectAttribute()
        {
            object o = new List<int> { 1, 2 };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(2);
            modification[0].Should().Be("1");
            modification[1].Should().Be("2");
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_String_ReturnsCorrectAttribute()
        {
            object o = "test";

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(1);
            modification[0].Should().Be("test");
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_True_ReturnsCorrectAttribute()
        {
            bool? o = true;

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(1);
            modification[0].Should().Be("TRUE");
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_False_ReturnsCorrectAttribute()
        {
            object o = false;

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(1);
            modification[0].Should().Be("FALSE");
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_X509Certificate_ReturnsCorrectAttribute()
        {
            var cert = new X509Certificate(Resources.cert);

            var modification = cert.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(1);
            modification[0].CastTo<byte[]>().Should().ContainInOrder(cert.GetRawCertData());
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_X509Certificate2_ReturnsCorrectAttribute()
        {
            var cert = new X509Certificate2(Resources.cert);

            var modification = cert.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(1);
            modification[0].CastTo<byte[]>().Should().ContainInOrder(cert.GetRawCertData());
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_EnumerableX509Certificate_ReturnsCorrectAttribute()
        {
            var cert = new[] { new X509Certificate(Resources.cert), new X509Certificate(Resources.cert) };

            var modification = cert.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(2);
            modification[0].CastTo<byte[]>().Should().ContainInOrder(cert[0].GetRawCertData());
            modification[1].CastTo<byte[]>().Should().ContainInOrder(cert[1].GetRawCertData());
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_EnumerableX509Certificate2_ReturnsCorrectAttribute()
        {
            var cert = new[] { new X509Certificate2(Resources.cert), new X509Certificate2(Resources.cert) };

            var modification = cert.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be("x");
            modification.Count.Should().Be(2);
            modification[0].CastTo<byte[]>().Should().ContainInOrder(cert[0].GetRawCertData());
            modification[1].CastTo<byte[]>().Should().ContainInOrder(cert[1].GetRawCertData());
            modification.Operation.Should().Be(DirectoryAttributeOperation.Replace);
        }
    }
}