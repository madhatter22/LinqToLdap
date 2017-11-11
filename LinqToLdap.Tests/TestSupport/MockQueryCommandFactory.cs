using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands;
using LinqToLdap.QueryCommands.Options;

namespace LinqToLdap.Tests.TestSupport
{
    public class MockQueryCommandFactory : IQueryCommandFactory
    {
        public QueryCommandType Type { get; set; }
        public IQueryCommandOptions Options { get; set; }
        public IObjectMapping Mapping { get; set; }

        public IQueryCommand QueryCommandToReturn { get; set; }

        public IQueryCommand GetCommand(QueryCommandType type, IQueryCommandOptions options, IObjectMapping mapping)
        {
            Type = type;
            Options = options;
            Mapping = mapping;

            return QueryCommandToReturn;
        }

        public string Filter
        {
            get
            {
                return Options != null ? Options.Filter : null;
            }
        }
    }
}
