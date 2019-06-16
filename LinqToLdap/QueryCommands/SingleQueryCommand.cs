using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;
using System;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.QueryCommands
{
    internal class SingleQueryCommand : QueryCommand
    {
        public SingleQueryCommand(IQueryCommandOptions options, IObjectMapping mapping)
            : base(options, mapping, true)
        {
        }

        public override object Execute(DirectoryConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
        {
            if (Options.YieldNoResults)
                throw new InvalidOperationException("Single returned 0 results due to a locally evaluated condition.");

            SetDistinguishedName(namingContext);
            SearchRequest.Scope = scope;
            if (Options.SortingOptions != null)
            {
                if (GetControl<SortRequestControl>(SearchRequest.Controls) != null)
                    throw new InvalidOperationException("Only one sort request control can be sent to the server");

                SearchRequest.Controls.Add(new SortRequestControl(Options.SortingOptions.Keys) { IsCritical = false });
            }
            if (GetControl<PageResultRequestControl>(SearchRequest.Controls) != null)
            {
                throw new InvalidOperationException("Only one page request control can be sent to the server.");
            }
            if (pagingEnabled && !Options.WithoutPaging)
            {
                SearchRequest.Controls.Add(new PageResultRequestControl(2));
            }

            if (log != null && log.TraceEnabled) log.Trace(SearchRequest.ToLogString());

            var response = connection.SendRequest(SearchRequest) as SearchResponse;

            response.AssertSuccess();

            if (response.Entries.Count != 1)
            {
                throw new InvalidOperationException(string.Format("Single returned {0} results for '{1}' against '{2}'", response.Entries.Count, SearchRequest.Filter, SearchRequest.DistinguishedName));
            }

            return Options.GetTransformer().Transform(response.Entries[0]);
        }

#if !NET35 && !NET40

        public override System.Threading.Tasks.Task<object> ExecuteAsync(LdapConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
        {
            throw new NotImplementedException();
        }

#endif
    }
}