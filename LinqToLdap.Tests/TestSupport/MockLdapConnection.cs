using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;

namespace LinqToLdap.Tests.TestSupport
{
    public class MockLdapConnection : LdapConnection
    {
        public MockLdapConnection(Dictionary<Type, DirectoryResponse> requestResposnes = null) : base("localhost")
        {
            RequestResponses = requestResposnes ?? new Dictionary<Type, DirectoryResponse>();
            SentRequests = new List<DirectoryRequest>();
        }

        public Exception ExceptionToThrow { get; set; }
        public Dictionary<Type, DirectoryResponse> RequestResponses { get; private set; }
        public List<DirectoryRequest> SentRequests { get; private set; } 

        public override DirectoryResponse SendRequest(DirectoryRequest request)
        {
            if (ExceptionToThrow != null) throw ExceptionToThrow;
            SentRequests.Add(request);
            return RequestResponses[request.GetType()];
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            TimesDisposed += 1;
        }

        public int TimesDisposed { get; private set; }
    }
}
