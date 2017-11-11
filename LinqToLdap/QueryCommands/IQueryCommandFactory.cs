/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using LinqToLdap.Mapping;
using LinqToLdap.QueryCommands.Options;

namespace LinqToLdap.QueryCommands
{
    internal interface IQueryCommandFactory
    {
        IQueryCommand GetCommand(QueryCommandType type, IQueryCommandOptions options, IObjectMapping mapping);
    }
}
