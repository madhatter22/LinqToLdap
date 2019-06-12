using LinqToLdap.Mapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Extensions
{
    [TestClass]
    [DirectorySchema("name")]
    public class TypeExtensionsTest
    {
        [TestMethod]
        public void IsAnonymous_NonAnonymousClass_ReturnsFalse()
        {
            GetType().IsAnonymous().Should().Be.False();
        }

        [TestMethod]
        public void IsAnonymous_AnonymousClass_ReturnsTrue()
        {
            var anon = new {Prop = ""};

            anon.GetType().IsAnonymous().Should().Be.True();
        }

        [TestMethod]
        public void HasDirectorySchema_ClassWithDirectorySchema_ReturnsTrue()
        {
            GetType().HasDirectorySchema().Should().Be.True();
        }

        [TestMethod]
        public void HasDirectorySchema_ClassWithoutDirectorySchema_ReturnsFalse()
        {
            var anon = new { Prop = "" };

            anon.GetType().HasDirectorySchema().Should().Be.False();
        }
    }
}
