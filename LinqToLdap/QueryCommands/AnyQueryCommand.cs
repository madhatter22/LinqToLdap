using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;
using System;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.QueryCommands
{
    internal class AnyQueryCommand : QueryCommand
    {
        public AnyQueryCommand(IQueryCommandOptions options, IObjectMapping mapping)
            : base(options, mapping, false)
        {
        }

        public override object Execute(DirectoryConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
        {
            if (Options.YieldNoResults) return false;

            BuildRequest(scope, maxPageSize, pagingEnabled, log, namingContext);

            var response = connection.SendRequest(SearchRequest) as SearchResponse;

            return HandleResponse(response);
        }

#if !NET35 && !NET40

        public override async System.Threading.Tasks.Task<object> ExecuteAsync(LdapConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
        {
            if (Options.YieldNoResults) return false;

            BuildRequest(scope, maxPageSize, pagingEnabled, log, namingContext);

            return await System.Threading.Tasks.Task.Factory.FromAsync(
                (callback, state) =>
                {
                    return connection.BeginSendRequest(SearchRequest, PartialResultProcessing.ReturnPartialResultsAndNotifyCallback, callback, state);
                },
                (asyncresult) =>
                {
                    var response = (SearchResponse)connection.EndSendRequest(asyncresult);
                    return HandleResponse(response);
                },
                null
            );
        }

#endif

        private void BuildRequest(SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
        {
            SetDistinguishedName(namingContext);
            SearchRequest.Scope = scope;
            if (GetControl<PageResultRequestControl>(SearchRequest.Controls) != null)
            {
                throw new InvalidOperationException("Only one page request control can be sent to the server.");
            }
            if (pagingEnabled && !Options.WithoutPaging)
            {
                SearchRequest.Controls.Add(new PageResultRequestControl(1));
            }

            SearchRequest.TypesOnly = true;
            SearchRequest.Attributes.Add("cn");

            if (log != null && log.TraceEnabled) log.Trace(SearchRequest.ToLogString());
        }

        private object HandleResponse(SearchResponse response)
        {
            response.AssertSuccess();

            return response.Entries.Count > 0;
        }
    }
}