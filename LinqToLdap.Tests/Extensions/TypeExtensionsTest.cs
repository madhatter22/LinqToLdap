using LinqToLdap.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Extensions
{
    [TestClass]
    [DirectorySchema("name")]
    public class TypeExtensionsTest
    {
        [TestMethod]
        public void IsAnonymous_NonAnonymousClass_ReturnsFalse()
        {
            GetType().IsAnonymous().Should().BeFalse();
        }

        [TestMethod]
        public void IsAnonymous_AnonymousClass_ReturnsTrue()
        {
            var anon = new {Prop = ""};

            anon.GetType().IsAnonymous().Should().BeTrue();
        }

        [TestMethod]
        public void HasDirectorySchema_ClassWithDirectorySchema_ReturnsTrue()
        {
            GetType().HasDirectorySchema().Should().BeTrue();
        }

        [TestMethod]
        public void HasDirectorySchema_ClassWithoutDirectorySchema_ReturnsFalse()
        {
            var anon = new { Prop = "" };

            anon.GetType().HasDirectorySchema().Should().BeFalse();
        }
    }
}
