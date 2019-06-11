namespace LinqToLdap.QueryCommands.Options
{
    internal class PagingOptions : IPagingOptions
    {
        public PagingOptions(int pageSize, byte[] nextPage)
        {
            NextPage = nextPage;
            PageSize = pageSize;
        }

        public byte[] NextPage { get; private set; }
        public int PageSize { get; private set; }
    }
}