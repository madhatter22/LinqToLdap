using System.DirectoryServices.Protocols;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;

namespace LinqToLdap.Tests.TestSupport
{
    public class DirectoryControlFactory
    {
        public static PageResultResponseControl CreatePageResponse(byte[] cookie, byte[] controlValue, int count = 0, bool criticality = true)
        {
            return typeof (PageResultResponseControl)
                .Create<PageResultResponseControl>(new object[] { count, cookie, criticality, controlValue });
        }
    }
}
