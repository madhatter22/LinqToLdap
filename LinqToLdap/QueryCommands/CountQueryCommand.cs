/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System.DirectoryServices.Protocols;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;

namespace LinqToLdap.QueryCommands
{
    
    internal class CountQueryCommand : QueryCommand
    {
        public CountQueryCommand(IQueryCommandOptions options, IObjectMapping mapping)
            : base(options, mapping, false)
        {
        }

        public override object Execute(DirectoryConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
        {
            if (Options.YieldNoResults) return 0;

            SetDistinguishedName(namingContext);
            SearchRequest.Scope = scope;

            int index = -1;
            if (pagingEnabled && !Options.WithoutPaging)
            {
                var pagedRequest = GetControl<PageResultRequestControl>(SearchRequest.Controls);
                if (pagedRequest != null)
                {
                    index = SearchRequest.Controls.IndexOf(pagedRequest);
                }

                if (pagedRequest == null)
                {
                    pagedRequest = new PageResultRequestControl(maxPageSize);
                    index = SearchRequest.Controls.Add(pagedRequest);
                }
            }
            
            SearchRequest.TypesOnly = true;
            SearchRequest.Attributes.Add("distinguishedname");

            if (log != null && log.TraceEnabled) log.Trace(SearchRequest.ToLogString());
            var response = connection.SendRequest(SearchRequest) as SearchResponse;
            response.AssertSuccess();

            int count = response.Entries.Count;
            if (pagingEnabled && !Options.WithoutPaging)
            {
                var pageResultResponseControl = GetControl<PageResultResponseControl>(response.Controls);
                bool hasResults = pageResultResponseControl != null && pageResultResponseControl.Cookie.Length > 0;
                while (hasResults)
                {
                    SearchRequest.Controls[index] = new PageResultRequestControl(pageResultResponseControl.Cookie);

                    if (log != null && log.TraceEnabled) log.Trace(SearchRequest.ToLogString());
                    response = connection.SendRequest(SearchRequest) as SearchResponse;
                    response.AssertSuccess();
                    pageResultResponseControl = GetControl<PageResultResponseControl>(response.Controls);
                    hasResults = pageResultResponseControl != null && pageResultResponseControl.Cookie.Length > 0;
                    count += response.Entries.Count;
                }
            }

            if (Options.IsLongCount)
            {
                return (long) count;
            }

            return count;
        }
    }
}
