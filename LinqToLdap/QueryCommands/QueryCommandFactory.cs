/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;

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

    /// <summary>
    /// Indicates the type of command to create.
    /// </summary>
    public enum QueryCommandType
    {
        /// <summary>
        /// Represents a standard command
        /// </summary>
        StandardCommand = 0,
        /// <summary>
        /// Any command
        /// </summary>
        AnyCommand = 1,
        /// <summary>
        /// First / FirstOrDefault command
        /// </summary>
        FirstOrDefaultCommand = 2,
        /// <summary>
        /// Single command
        /// </summary>
        SingleCommand = 3,
        /// <summary>
        /// SingleOrDefault command
        /// </summary>
        SingleOrDefaultCommand = 4,
        /// <summary>
        /// Count command
        /// </summary>
        CountCommand = 5,
        /// <summary>
        /// GetRequest command
        /// </summary>
        GetRequestCommand = 6
    }
}
