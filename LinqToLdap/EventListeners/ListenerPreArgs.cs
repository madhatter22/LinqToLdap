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

namespace LinqToLdap.EventListeners
{
    /// <summary>
    /// Arguments passed for <see cref="IEventListener"/>s. The arguments used by this class are <see cref="WeakReference"/>s.
    /// </summary>
    /// <typeparam name="TObject">The instance for the event.</typeparam>
    /// <typeparam name="TRequest">The partially populated request to be sent to the directory.</typeparam>
    public class ListenerPreArgs<TObject, TRequest> where TRequest : DirectoryRequest where TObject : class
    {
#if !NET45
        private readonly WeakReference _connection;
        private readonly WeakReference _entry;
#else
        private readonly WeakReference<LdapConnection> _connection;
        private readonly WeakReference<TObject> _entry;
#endif

        internal ListenerPreArgs(TObject entry, TRequest request, LdapConnection connection)
        {
#if !NET45
            _entry = new WeakReference(entry);
            _connection = new WeakReference(connection);
#else
            _entry = new WeakReference<TObject>(entry);
            _connection = new WeakReference<LdapConnection>(connection);
#endif
            Request = request;
        }

        /// <summary>
        /// The entry.
        /// </summary>
        /// 
        public TObject Entry
        {
            get
            {
                
#if !NET45
                return _entry.Target as TObject;
#else
                TObject target;
                return _entry.TryGetTarget(out target) ? target : default(TObject);
#endif
            }
        }

        /// <summary>
        /// The <see cref="DirectoryRequest"/>.
        /// </summary>
        public TRequest Request { get; private set; }

        /// <summary>
        /// The connection that will be used when sending the <see cref="Request"/>.
        /// </summary>
        public LdapConnection Connection
        {
            get
            {
#if !NET45
                return _connection.Target as LdapConnection;
#else
                LdapConnection target;
                return _connection.TryGetTarget(out target) ? target : null;
#endif
            }
        }
    }
}
