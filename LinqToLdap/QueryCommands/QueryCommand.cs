/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.DirectoryServices.Protocols;
using System.Linq;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;

namespace LinqToLdap.QueryCommands
{
    internal abstract class QueryCommand : IQueryCommand
    {
        protected readonly SearchRequest SearchRequest;
        protected readonly IQueryCommandOptions Options;
        protected readonly IObjectMapping Mapping;

        protected QueryCommand(IQueryCommandOptions options, IObjectMapping mapping, bool initializeAttributes)
        {
            Options = options;
            Mapping = mapping;
            SearchRequest = new SearchRequest {Filter = options.Filter};
            if (Options.Controls != null)
            {
                SearchRequest.Controls.AddRange(Options.Controls.ToArray());
            }
            if (initializeAttributes)
            {
                InitializeAttributes();
            }
        }

        private void InitializeAttributes()
        {
            if (!Mapping.HasCatchAllMapping)
            {
                var attributes = Mapping.HasSubTypeMappings
                    ? Options.AttributesToLoad.Values
                        .Union(new[] {"objectClass"}, StringComparer.OrdinalIgnoreCase)
                        .ToArray()
                    : Options.AttributesToLoad.Values.ToArray();
                SearchRequest.Attributes.AddRange(attributes);
            }
        }

        protected virtual T GetControl<T>(DirectoryControl[] controls) where T : class
        {
            if (controls == null || controls.Length == 0) return default(T);

            return controls.FirstOrDefault(c => c is T) as T;
        }

        protected virtual T GetControl<T>(DirectoryControlCollection controls) where T : class
        {
            if (controls == null || controls.Count == 0) return default(T);

            return controls.OfType<T>().FirstOrDefault();
        }

        public abstract object Execute(DirectoryConnection connection, SearchScope scope, int maxPageSize, bool pagingEnabled, ILinqToLdapLogger log = null, string namingContext = null);

        protected void SetDistinguishedName(string namingContext)
        {
            SearchRequest.DistinguishedName = namingContext ?? Mapping.NamingContext;
        }
        
        public override string ToString()
        {
            return SearchRequest.ToLogString();
        }
    }
}
