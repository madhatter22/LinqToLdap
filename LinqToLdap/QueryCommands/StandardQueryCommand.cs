using LinqToLdap.Collections;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.QueryCommands
{
    internal class StandardQueryCommand : QueryCommand
    {
        public StandardQueryCommand(IQueryCommandOptions options, IObjectMapping mapping)
            : base(options, mapping, true)
        {
        }

        public override object Execute(DirectoryConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
        {
            SetDistinguishedName(namingContext);
            SearchRequest.Scope = scope;

            if (Options.SortingOptions != null)
            {
                if (GetControl<SortRequestControl>(SearchRequest.Controls) != null)
                    throw new InvalidOperationException("Only one sort request control can be sent to the server");

                SearchRequest.Controls.Add(new SortRequestControl(Options.SortingOptions.Keys) { IsCritical = false });
            }

            PageResultRequestControl pageRequest;
            if (Options.WithoutPaging || !pagingEnabled || (pageRequest = GetControl<PageResultRequestControl>(SearchRequest.Controls)) == null && Options.PagingOptions == null)
            {
                return Options.SkipSize.HasValue || GetControl<VlvRequestControl>(SearchRequest.Controls) != null
                    ? HandleVlvRequest(connection, maxPageSize, log)
                    : HandleStandardRequest(connection, log, maxPageSize,
                        Options.WithoutPaging == false && pagingEnabled);
            }

            if (Options.PagingOptions != null && pageRequest != null)
            {
                throw new InvalidOperationException("Only one page request control can be sent to the server.");
            }
            return HandlePagedRequest(connection, pageRequest, log);
        }

        public virtual object HandleStandardRequest(DirectoryConnection connection, ILinqToLdapLogger log, int maxSize, bool pagingEnabled)
        {
            if (Options.YieldNoResults)
                return Activator.CreateInstance(typeof(List<>).MakeGenericType(Options.GetEnumeratorReturnType()));

            int pageSize = 0;
            int index = 0;
            if (pagingEnabled)
            {
                int maxPageSize = Options.PageSize ?? maxSize;
                pageSize = Options.TakeSize.HasValue ? Math.Min(Options.TakeSize.Value, maxPageSize) : maxPageSize;

                index = SearchRequest.Controls.Add(new PageResultRequestControl(pageSize));
            }

            if (log != null && log.TraceEnabled) log.Trace(SearchRequest.ToLogString());
            var response = connection.SendRequest(SearchRequest) as SearchResponse;
            response.AssertSuccess();

            var list = new List<SearchResultEntry>();

            list.AddRange(response.Entries.GetRange());

            if (pagingEnabled)
            {
                if (response.Entries.Count > 0)
                {
                    var pageResultResponseControl = GetControl<PageResultResponseControl>(response.Controls);
                    bool hasMoreResults = pageResultResponseControl != null && pageResultResponseControl.Cookie.Length > 0 && (!Options.TakeSize.HasValue || list.Count < Options.TakeSize.Value);
                    while (hasMoreResults)
                    {
                        SearchRequest.Controls[index] = new PageResultRequestControl(pageSize) { Cookie = pageResultResponseControl.Cookie };
                        if (log != null && log.TraceEnabled) log.Trace(SearchRequest.ToLogString());
                        response = connection.SendRequest(SearchRequest) as SearchResponse;
                        response.AssertSuccess();
                        pageResultResponseControl = GetControl<PageResultResponseControl>(response.Controls);
                        hasMoreResults = pageResultResponseControl != null && pageResultResponseControl.Cookie.Length > 0 && (!Options.TakeSize.HasValue || list.Count <= Options.TakeSize.Value);

                        list.AddRange(response.Entries.GetRange());
                    }
                }
            }

            AssertSortSuccess(response.Controls);

            if (Options.TakeSize.HasValue && list.Count > Options.TakeSize.Value)
            {
                var size = Options.TakeSize.Value;
                list.RemoveRange(size, list.Count - size);
            }

            var enumerator = Options.GetEnumerator(list);

            return Activator.CreateInstance(typeof(List<>).MakeGenericType(Options.GetEnumeratorReturnType()),
                                            new[] { enumerator });
        }

        public virtual object HandlePagedRequest(DirectoryConnection connection, PageResultRequestControl pageRequest, ILinqToLdapLogger log)
        {
            if (Options.YieldNoResults)
            {
                var bindingAttr = new[]
                            {
                                pageRequest.PageSize,
                                null,
                                Options.GetEnumerator(),
                                null
                            };

                return Activator.CreateInstance(typeof(LdapPage<>).MakeGenericType(Options.GetEnumeratorReturnType()), bindingAttr);
            }

            if (pageRequest == null)
            {
                pageRequest = new PageResultRequestControl(Options.PagingOptions.PageSize)
                {
                    Cookie = Options.PagingOptions.NextPage
                };

                SearchRequest.Controls.Add(pageRequest);
            }

            if (log != null && log.TraceEnabled) log.Trace(SearchRequest.ToLogString());
            var response = connection.SendRequest(SearchRequest) as SearchResponse;

            response.AssertSuccess();

            AssertSortSuccess(response.Controls);

            var nextPage = GetControl<PageResultResponseControl>(response.Controls);
            var parameters = new[]
                                 {
                                     pageRequest.PageSize,
                                     nextPage?.Cookie,
                                     Options.GetEnumerator(response.Entries),
                                     Options.Filter
                                 };
            return Activator.CreateInstance(
                typeof(LdapPage<>).MakeGenericType(Options.GetEnumeratorReturnType()),
                parameters);
        }

        public virtual object HandleVlvRequest(DirectoryConnection connection, int maxSize, ILinqToLdapLogger log)
        {
            if (Options.YieldNoResults)
            {
                var bindingAttr = new[]
                            {
                                0,
                                Options.GetEnumerator()
                            };

                return Activator.CreateInstance(typeof(VirtualListView<>).MakeGenericType(Options.GetEnumeratorReturnType()), bindingAttr);
            }

            if (GetControl<SortRequestControl>(SearchRequest.Controls) == null)
                throw new InvalidOperationException("Virtual List Views require a sort operation. Please include an OrderBy in your query.");

            var skip = Options.SkipSize.GetValueOrDefault();
            if (GetControl<VlvRequestControl>(SearchRequest.Controls) == null)
            {
                VlvRequestControl vlvRequest =
                new VlvRequestControl(0, Options.TakeSize.GetValueOrDefault(maxSize) - 1, skip + 1) { IsCritical = false };

                SearchRequest.Controls.Add(vlvRequest);
            }

            if (log != null && log.TraceEnabled) log.Trace(SearchRequest.ToLogString());
            var response = connection.SendRequest(SearchRequest) as SearchResponse;

            response.AssertSuccess();

            AssertSortSuccess(response.Controls);

            var vlvResponse = GetControl<VlvResponseControl>(response.Controls);

            if (vlvResponse == null)
                throw new InvalidOperationException("The server does not support Virtual List Views. Skip cannot be used. Please use standard paging.");
            var parameters = new[]
                                 {
                                     vlvResponse.ContentCount,
                                     vlvResponse.ContextId,
                                     vlvResponse.TargetPosition,
                                     Options.GetEnumerator(response.Entries)
                                 };
            return Activator.CreateInstance(
                typeof(VirtualListView<>).MakeGenericType(Options.GetEnumeratorReturnType()),
                parameters);
        }

        public void AssertSortSuccess(DirectoryControl[] controls)
        {
            var control = GetControl<SortResponseControl>(controls);

            if (control != null && control.Result != ResultCode.Success)
            {
                throw new LdapException(
                    string.Format("Sort request returned {0}", control.Result));
            }
        }
    }
}