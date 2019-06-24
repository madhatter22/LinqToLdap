using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;
using System;
using System.DirectoryServices.Protocols;

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

            var index = BuildRequest(scope, maxPageSize, pagingEnabled, log, namingContext);
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
                return (long)count;
            }

            return count;
        }

#if !NET35 && !NET40

        public override async System.Threading.Tasks.Task<object> ExecuteAsync(LdapConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
        {
            if (Options.YieldNoResults) return 0;

            var index = BuildRequest(scope, maxPageSize, pagingEnabled, log, namingContext);
            SearchResponse response = null;
            int count = 0;
            bool hasResults = false;
            PageResultResponseControl pageResultResponseControl = null;
            void handleRespnse(SearchResponse r)
            {
                r.AssertSuccess();

                pageResultResponseControl = GetControl<PageResultResponseControl>(r.Controls);
                hasResults = pageResultResponseControl != null && pageResultResponseControl.Cookie.Length > 0;
                count += r.Entries.Count;
            }
#if NET45
            await System.Threading.Tasks.Task.Factory.FromAsync(
                (callback, state) =>
                {
                    return connection.BeginSendRequest(SearchRequest, Options.AsyncProcessing, callback, state);
                },
                (asyncresult) =>
                {
                    response = (SearchResponse)connection.EndSendRequest(asyncresult);
                    handleRespnse(response);
                },
                null
            );
#else
            response = await System.Threading.Tasks.Task.Run(() => connection.SendRequest(SearchRequest) as SearchResponse);
            handleRespnse(response);
#endif

            if (pagingEnabled && !Options.WithoutPaging)
            {
                while (hasResults)
                {
                    SearchRequest.Controls[index] = new PageResultRequestControl(pageResultResponseControl.Cookie);

                    if (log != null && log.TraceEnabled) log.Trace(SearchRequest.ToLogString());
#if NET45
                    await System.Threading.Tasks.Task.Factory.FromAsync(
                        (callback, state) =>
                        {
                            return connection.BeginSendRequest(SearchRequest, Options.AsyncProcessing, callback, state);
                        },
                        (asyncresult) =>
                        {
                            response = (SearchResponse)connection.EndSendRequest(asyncresult);
                            handleRespnse(response);
                        },
                        null
                    );
#else
                    response = await System.Threading.Tasks.Task.Run(() => connection.SendRequest(SearchRequest) as SearchResponse);

                    handleRespnse(response);
#endif
                }
            }

            if (Options.IsLongCount)
            {
                return (long)count;
            }

            return count;
        }

#endif

        private int BuildRequest(SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
        {
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

            return index;
        }

        private object HandleResult(IAsyncResult asyncresult, LdapConnection connection)
        {
            var response = (SearchResponse)connection.EndSendRequest(asyncresult);
            response.AssertSuccess();

            return response.Entries.Count > 0;
        }
    }
}