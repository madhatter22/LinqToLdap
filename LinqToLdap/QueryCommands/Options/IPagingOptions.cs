namespace LinqToLdap.QueryCommands.Options
{
    /// <summary>
    /// Interface for paging options from a Query.
    /// </summary>
    public interface IPagingOptions
    {
        /// <summary>
        /// The next page.
        /// </summary>
        byte[] NextPage { get; }

        /// <summary>
        /// The size of the page.
        /// </summary>
        int PageSize { get; }
    }
}