using LinqToLdap.Collections;
using LinqToLdap.Helpers;
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
                return ObjectActivator.CreateGenericInstance(typeof(List<>), Options.GetEnumeratorReturnType(), null, null);

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

            return ObjectActivator.CreateGenericInstance(typeof(List<>), Options.GetEnumeratorReturnType(), new[] { enumerator }, null);
        }

        public virtual object HandlePagedRequest(DirectoryConnection connection, PageResultRequestControl pageRequest, ILinqToLdapLogger log)
        {
            object enumerator = null;
            if (Options.YieldNoResults)
            {
                enumerator = Options.GetEnumerator();
                var bindingAttr = new[]
                            {
                                pageRequest.PageSize,
                                null,
                                enumerator,
                                null
                            };

                return ObjectActivator.CreateGenericInstance(typeof(LdapPage<>), Options.GetEnumeratorReturnType(), bindingAttr, null);
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

            enumerator = Options.GetEnumerator(response.Entries);
            var nextPage = GetControl<PageResultResponseControl>(response.Controls);
            var parameters = new[]
                                 {
                                     pageRequest.PageSize,
                                     nextPage?.Cookie,
                                     enumerator,
                                     Options.Filter
                                 };
            return ObjectActivator.CreateGenericInstance(typeof(LdapPage<>), Options.GetEnumeratorReturnType(), parameters, null);
        }

        public virtual object HandleVlvRequest(DirectoryConnection connection, int maxSize, ILinqToLdapLogger log)
        {
            if (Options.YieldNoResults)
            {
                var enumerator = Options.GetEnumerator();
                var bindingAttr = new[]
                            {
                                0,
                                null,
                                0,
                                enumerator
                            };

                return ObjectActivator.CreateGenericInstance(typeof(VirtualListView<>), Options.GetEnumeratorReturnType(), bindingAttr, null);
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

            return ObjectActivator.CreateGenericInstance(typeof(VirtualListView<>), Options.GetEnumeratorReturnType(), parameters, null);
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

#if !NET35 && !NET40

        public override async System.Threading.Tasks.Task<object> ExecuteAsync(LdapConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null)
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
                return await (Options.SkipSize.HasValue || GetControl<VlvRequestControl>(SearchRequest.Controls) != null
                    ? HandleVlvRequestAsync(connection, maxPageSize, log)
                    : HandleStandardRequestAsync(connection, log, maxPageSize,
                        Options.WithoutPaging == false && pagingEnabled));
            }

            if (Options.PagingOptions != null && pageRequest != null)
            {
                throw new InvalidOperationException("Only one page request control can be sent to the server.");
            }
            return await HandlePagedRequestAsync(connection, pageRequest, log);
        }

        private async System.Threading.Tasks.Task<object> HandleStandardRequestAsync(LdapConnection connection, ILinqToLdapLogger log, int maxSize, bool pagingEnabled)
        {
            if (Options.YieldNoResults)
                return ObjectActivator.CreateGenericInstance(typeof(List<>), Options.GetEnumeratorReturnType(), null, null);

            int pageSize = 0;
            int index = 0;
            if (pagingEnabled)
            {
                int maxPageSize = Options.PageSize ?? maxSize;
                pageSize = Options.TakeSize.HasValue ? Math.Min(Options.TakeSize.Value, maxPageSize) : maxPageSize;

                index = SearchRequest.Controls.Add(new PageResultRequestControl(pageSize));
            }

            if (log != null && log.TraceEnabled) log.Trace(SearchRequest.ToLogString());

            var list = new List<SearchResultEntry>();
            SearchResponse response = null;
            await System.Threading.Tasks.Task.Factory.FromAsync(
                (callback, state) =>
                {
                    return connection.BeginSendRequest(SearchRequest, PartialResultProcessing.ReturnPartialResultsAndNotifyCallback, callback, state);
                },
                (asyncresult) =>
                {
                    response = (SearchResponse)connection.EndSendRequest(asyncresult);
                    response.AssertSuccess();

                    list.AddRange(response.Entries.GetRange());
                },
                null
            );

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

                        await System.Threading.Tasks.Task.Factory.FromAsync(
                            (callback, state) =>
                            {
                                return connection.BeginSendRequest(SearchRequest, PartialResultProcessing.ReturnPartialResultsAndNotifyCallback, callback, state);
                            },
                            (asyncresult) =>
                            {
                                response = (SearchResponse)connection.EndSendRequest(asyncresult);
                                response.AssertSuccess();

                                pageResultResponseControl = GetControl<PageResultResponseControl>(response.Controls);
                                hasMoreResults = pageResultResponseControl != null && pageResultResponseControl.Cookie.Length > 0 && (!Options.TakeSize.HasValue || list.Count <= Options.TakeSize.Value);

                                list.AddRange(response.Entries.GetRange());
                            },
                            null
                        );
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

            return ObjectActivator.CreateGenericInstance(typeof(List<>), Options.GetEnumeratorReturnType(), new[] { enumerator }, null);
        }

        private async System.Threading.Tasks.Task<object> HandleVlvRequestAsync(LdapConnection connection, int maxSize, ILinqToLdapLogger log)
        {
            if (Options.YieldNoResults)
            {
                var enumerator = Options.GetEnumerator();
                var bindingAttr = new[]
                            {
                                0,
                                null,
                                0,
                                enumerator
                            };

                return ObjectActivator.CreateGenericInstance(typeof(VirtualListView<>), Options.GetEnumeratorReturnType(), bindingAttr, null);
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

            return await System.Threading.Tasks.Task.Factory.FromAsync(
                (callback, state) =>
                {
                    return connection.BeginSendRequest(SearchRequest, PartialResultProcessing.ReturnPartialResultsAndNotifyCallback, callback, state);
                },
                (asyncresult) =>
                {
                    var response = (SearchResponse)connection.EndSendRequest(asyncresult);
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

                    return ObjectActivator.CreateGenericInstance(typeof(VirtualListView<>), Options.GetEnumeratorReturnType(), parameters, null);
                },
                null
            );
        }

        public virtual async System.Threading.Tasks.Task<object> HandlePagedRequestAsync(LdapConnection connection, PageResultRequestControl pageRequest, ILinqToLdapLogger log)
        {
            if (Options.YieldNoResults)
            {
                var enumerator = Options.GetEnumerator();
                var bindingAttr = new[]
                            {
                                pageRequest.PageSize,
                                null,
                                enumerator,
                                null
                            };

                return ObjectActivator.CreateGenericInstance(typeof(LdapPage<>), Options.GetEnumeratorReturnType(), bindingAttr, null);
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

            return await System.Threading.Tasks.Task.Factory.FromAsync(
                (callback, state) =>
                {
                    return connection.BeginSendRequest(SearchRequest, PartialResultProcessing.ReturnPartialResultsAndNotifyCallback, callback, state);
                },
                (asyncresult) =>
                {
                    var response = (SearchResponse)connection.EndSendRequest(asyncresult);
                    response.AssertSuccess();
                    AssertSortSuccess(response.Controls);

                    var enumerator = Options.GetEnumerator(response.Entries);
                    var nextPage = GetControl<PageResultResponseControl>(response.Controls);
                    var parameters = new[]
                    {
                        pageRequest.PageSize,
                        nextPage?.Cookie,
                        enumerator,
                        Options.Filter
                    };

                    return ObjectActivator.CreateGenericInstance(typeof(LdapPage<>), Options.GetEnumeratorReturnType(), parameters, null);
                },
                null
            );
        }

#endif
    }
}