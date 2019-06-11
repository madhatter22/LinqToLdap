using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;

namespace LinqToLdap.QueryCommands
{
    internal interface IQueryCommandFactory
    {
        IQueryCommand GetCommand(QueryCommandType type, IQueryCommandOptions options, IObjectMapping mapping);
    }
}