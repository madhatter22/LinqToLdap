/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

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
