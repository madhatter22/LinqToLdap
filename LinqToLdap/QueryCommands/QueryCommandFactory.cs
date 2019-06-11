using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;
using System;

namespace LinqToLdap.QueryCommands
{
    internal class QueryCommandFactory : IQueryCommandFactory
    {
        public IQueryCommand GetCommand(QueryCommandType type, IQueryCommandOptions options, IObjectMapping mapping)
        {
            switch (type)
            {
                case QueryCommandType.StandardCommand:
                    return new StandardQueryCommand(options, mapping);

                case QueryCommandType.AnyCommand:
                    return new AnyQueryCommand(options, mapping);

                case QueryCommandType.CountCommand:
                    return new CountQueryCommand(options, mapping);

                case QueryCommandType.FirstOrDefaultCommand:
                    return new FirstOrDefaultQueryCommand(options, mapping);

                case QueryCommandType.SingleCommand:
                    return new SingleQueryCommand(options, mapping);

                case QueryCommandType.SingleOrDefaultCommand:
                    return new SingleOrDefaultQueryCommand(options, mapping);

                case QueryCommandType.GetRequestCommand:
                    return new GetRequestCommand(options, mapping);

                default:
                    throw new NotSupportedException(string.Format("QueryCommandType '{0}' is not supported.", type));
            }
        }
    }
}