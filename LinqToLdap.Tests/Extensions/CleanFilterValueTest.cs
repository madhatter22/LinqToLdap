using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Extensions
{
    [TestClass]
    public class CleanFilterValueTest
    {
        [TestMethod]
        public void CleanFilterValue()
        {
            string value = "!&:|~a\\*()\u0000z";

            value.CleanFilterValue().Should().Be.EqualTo("\\21\\26\\3a\\7c\\7ea\\5c\\2a\\28\\29\\00z");
        }
    }
}
