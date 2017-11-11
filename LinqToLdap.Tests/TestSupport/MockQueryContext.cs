using LinqToLdap.Collections;
using LinqToLdap.TestSupport;

namespace LinqToLdap.Tests.TestSupport
{
    public class MockQueryContext
    {
        public MockQueryProvider ActiveProvider { get; private set; }

        public MockQuery<T> Query<T>()
        {
            ActiveProvider = new MockQueryProvider(null);
            var query = new MockQuery<T>(ActiveProvider);

            return query;
        }

        public MockQuery<IDirectoryAttributes> Query()
        {
            ActiveProvider = new MockQueryProvider(null);
            var query = new MockQuery<IDirectoryAttributes>(ActiveProvider);

            return query;
        }
    }
}
