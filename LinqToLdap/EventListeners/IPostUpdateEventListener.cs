/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System.DirectoryServices.Protocols;

namespace LinqToLdap.EventListeners
{
    /// <summary>
    /// The event raised after an update occurs.
    /// </summary>
    public interface IPostUpdateEventListener : IPostEventListener<object, ModifyRequest, ModifyResponse>, IUpdateEventListener
    {
    }
}