using System.DirectoryServices.Protocols;
using System.Linq;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands;
using LinqToLdap.QueryCommands.Options;

namespace LinqToLdap.Tests.TestSupport.QueryCommands
{
    internal class MockStandardQueryCommand : StandardQueryCommand
    {
        private bool _enablePagedRequestCall = true;
        private bool _enableStandardRequestCall = true;

        public MockStandardQueryCommand(IQueryCommandOptions options, IObjectMapping mapping) : base(options, mapping)
        {
        }

        public MockStandardQueryCommand DisablePagedRequest()
        {
            _enablePagedRequestCall = false;
            return this;
        }

        public MockStandardQueryCommand DisableStandardRequest()
        {
            _enableStandardRequestCall = false;
            return this;
        }

        public bool HandlePagedRequestCalled { get; set; }

        public bool HandleStandardRequestCalled { get; set; }

        public override object HandlePagedRequest(DirectoryConnection connection, PageResultRequestControl pageRequest, ILinqToLdapLogger log)
        {
            HandlePagedRequestCalled = true;
            return _enablePagedRequestCall ? base.HandlePagedRequest(connection, pageRequest, log) : null;
        }

        public override object HandleStandardRequest(DirectoryConnection connection, ILinqToLdapLogger log, int maxSize, bool pagingEnabled)
        {
            HandleStandardRequestCalled = true;
            return _enableStandardRequestCall ? base.HandleStandardRequest(connection, log, maxSize, pagingEnabled) : null;
        }

        public SearchRequest GetRequest()
        {
            return SearchRequest;
        }

        public DirectoryControl[] RequestControlsToSearch { get; set; }
        protected override T GetControl<T>(DirectoryControlCollection controls)
        {
            if (RequestControlsToSearch == null)
            {
                return base.GetControl<T>(controls);
            }
            return RequestControlsToSearch.FirstOrDefault(c => c is T) as T;
        }

        public DirectoryControl[] ResponseControlsToSearch { get; set; }
        protected override T GetControl<T>(DirectoryControl[] controls)
        {
            if (ResponseControlsToSearch == null)
            {
                return base.GetControl<T>(controls);
            }
            return ResponseControlsToSearch.FirstOrDefault(c => c is T) as T;
        }
    }
}
