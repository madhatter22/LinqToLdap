using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

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

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(0);
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_StringArray_ReturnsCorrectAttribute()
        {
            object o = new[] { "one", "two" };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(2);
            modification[0].Should().Be.EqualTo("one");
            modification[1].Should().Be.EqualTo("two");
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_StringList_ReturnsCorrectAttribute()
        {
            object o = new List<string> { "one", "two" };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(2);
            modification[0].Should().Be.EqualTo("one");
            modification[1].Should().Be.EqualTo("two");
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_ByteArray_ReturnsCorrectAttribute()
        {
            object o = new byte[] { 1, 2 };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(1);
            modification[0].Should().Be.EqualTo(o);
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_ListBytes_ReturnsCorrectAttribute()
        {
            object o = new List<byte> { 1, 2 };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(1);
            modification[0].As<byte[]>().Should().Have.SameSequenceAs(o as IEnumerable<byte>);
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_Guid_ReturnsCorrectAttribute()
        {
            object o = Guid.NewGuid();

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(1);
            modification[0].As<byte[]>().Should().Have.SameSequenceAs(o.CastTo<Guid>().ToByteArray());
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
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

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(1);
            modification[0].As<byte[]>().Should().Have.SameSequenceAs(bytes);
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_ListByteArrays_ReturnsCorrectAttribute()
        {
            var bytes1 = new byte[] { 1, 2 };
            var bytes2 = new byte[] { 3, 4 };
            object o = new[] { bytes1, bytes2 };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(2);
            modification[0].Should().Be.EqualTo(bytes1);
            modification[1].Should().Be.EqualTo(bytes2);
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_ArrayInts_ReturnsCorrectAttribute()
        {
            object o = new[] { 1, 2 };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(2);
            modification[0].Should().Be.EqualTo("1");
            modification[1].Should().Be.EqualTo("2");
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_ListInts_ReturnsCorrectAttribute()
        {
            object o = new List<int> { 1, 2 };

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(2);
            modification[0].Should().Be.EqualTo("1");
            modification[1].Should().Be.EqualTo("2");
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_String_ReturnsCorrectAttribute()
        {
            object o = "test";

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(1);
            modification[0].Should().Be.EqualTo("test");
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_True_ReturnsCorrectAttribute()
        {
            bool? o = true;

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(1);
            modification[0].Should().Be.EqualTo("TRUE");
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_False_ReturnsCorrectAttribute()
        {
            object o = false;

            var modification = o.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(1);
            modification[0].Should().Be.EqualTo("FALSE");
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_X509Certificate_ReturnsCorrectAttribute()
        {
            var cert = new X509Certificate(Properties.Resources.cert);

            var modification = cert.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(1);
            modification[0].As<byte[]>().Should().Have.SameSequenceAs(cert.GetRawCertData());
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_X509Certificate2_ReturnsCorrectAttribute()
        {
            var cert = new X509Certificate2(Properties.Resources.cert);

            var modification = cert.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(1);
            modification[0].As<byte[]>().Should().Have.SameSequenceAs(cert.GetRawCertData());
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_EnumerableX509Certificate_ReturnsCorrectAttribute()
        {
            var cert = new[] { new X509Certificate(Properties.Resources.cert), new X509Certificate(Properties.Resources.cert) };

            var modification = cert.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(2);
            modification[0].As<byte[]>().Should().Have.SameSequenceAs(cert[0].GetRawCertData());
            modification[1].As<byte[]>().Should().Have.SameSequenceAs(cert[1].GetRawCertData());
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }

        [TestMethod]
        public void ToDirectoryModification_EnumerableX509Certificate2_ReturnsCorrectAttribute()
        {
            var cert = new[] { new X509Certificate2(Properties.Resources.cert), new X509Certificate2(Properties.Resources.cert) };

            var modification = cert.ToDirectoryModification("x", DirectoryAttributeOperation.Replace);

            modification.Name.Should().Be.EqualTo("x");
            modification.Count.Should().Be.EqualTo(2);
            modification[0].As<byte[]>().Should().Have.SameSequenceAs(cert[0].GetRawCertData());
            modification[1].As<byte[]>().Should().Have.SameSequenceAs(cert[1].GetRawCertData());
            modification.Operation.Should().Be.EqualTo(DirectoryAttributeOperation.Replace);
        }
    }
}
