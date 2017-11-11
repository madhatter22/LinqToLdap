/*
 * LINQ to LDAP
 * http://linqtoldap.codeplex.com/
 * 
 * Copyright Alan Hatter (C) 2010-2014
 
 * 
 * This project is subject to licensing restrictions. Visit http://linqtoldap.codeplex.com/license for more information.
 */

using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Reflection;
using LinqToLdap.EventListeners;
using LinqToLdap.Exceptions;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;

namespace LinqToLdap
{
    /// <summary>
    /// Class for configuring LINQ to LDAP.
    /// </summary>
    public class LdapConfiguration : ILdapConfiguration
    {
        private int _serverMaxPageSize = 500;
        private bool _pagingEnabled = true;
#if NET35
        private readonly Collections.SafeList<IEventListener> _listeners = new Collections.SafeList<IEventListener>();
#else
        private readonly System.Collections.Concurrent.ConcurrentBag<IEventListener> _listeners = new System.Collections.Concurrent.ConcurrentBag<IEventListener>();
#endif

        /// <summary>
        /// Constructs an instance of this class and initializes <see cref="Mapper"/>.
        /// </summary>
        public LdapConfiguration()
        {
            Mapper = new DirectoryMapper();
        }

        /// <summary>
        /// Deconstructor that sets related properties to null.
        /// </summary>
        ~LdapConfiguration()
        {
            ConnectionFactory = null;
            Log = null;
            Mapper = null;
        }
        
        /// <summary>
        /// The configured connection factory to be used for all 
        /// <see cref="DirectoryContext"/>s that don't explicitly get an <see cref="LdapConnection"/>.
        /// </summary>
        public ILdapConnectionFactory ConnectionFactory { get; private set; }

        /// <summary>
        /// Used for writing <see cref="DirectoryRequest"/> and <see cref="DirectoryResponse"/> information to a log.
        /// </summary>
        public ILinqToLdapLogger Log { get; private set; }

        /// <summary>
        /// Class responsible for mapping objects to directory entries.
        /// </summary>
        public IDirectoryMapper Mapper { get; set; }

        /// <summary>
        /// Get all event listeners of type <typeparamref name="TListener"/> registered with this configuration.
        /// </summary>
        /// <typeparam name="TListener">The type of listeners to retrieve.</typeparam>
        /// <returns></returns>
        public IEnumerable<TListener> GetListeners<TListener>() where TListener : IEventListener
        {
            return _listeners.OfType<TListener>();
        }

        /// <summary>
        /// Get all event listeners registered with this configuration.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IEventListener> GetListeners()
        {
#if NET35
            return _listeners.ToList();
#else
            return _listeners;
#endif
        }

        /// <summary>
        /// Registers the listeners with this configuration.
        /// </summary>
        /// <param name="listeners">Implementations of <see cref="IEventListener"/>.</param>
        public void RegisterListeners(params IEventListener[] listeners)
        {
            if (listeners == null) throw new ArgumentNullException("listeners");
            foreach (var listener in listeners)
            {
                _listeners.Add(listener);
            }
        }

        /// <summary>
        /// Static storage for a global configuration.
        /// </summary>
        public static ILdapConfiguration Configuration { get; private set; }

        /// <summary>
        /// Get the server max page size.  Default is 500.  Change this value with <see cref="MaxPageSizeIs"/>.
        /// </summary>
        public int ServerMaxPageSize { get { return _serverMaxPageSize; } }

        /// <summary>
        /// Indicates if paging is enabled.
        /// </summary>
        public bool PagingEnabled
        {
            get { return _pagingEnabled; }
        }

        /// <summary>
        /// Indicates the maximum number of results the server can return at a time.
        /// </summary>
        /// <param name="size">The maximum amount.</param>
        /// <returns></returns>
        public LdapConfiguration MaxPageSizeIs(int size)
        {
            _serverMaxPageSize = size;
            return this;
        }

        /// <summary>
        /// Disables paging for all <see cref="DirectoryContext"/>s using this configuration.
        /// </summary>
        /// <returns></returns>
        public LdapConfiguration DisablePaging()
        {
            _pagingEnabled = false;
            return this;
        }
        
        /// <summary>
        /// Adds the mapping.
        /// </summary>
        /// <param name="classMap">The mapping for the class</param>
        /// <param name="objectCategory">The object category for the object.</param>
        /// <param name="includeObjectCategory">
        /// Indicates if the object category should be included in all queries.
        /// </param>
        /// <param name="namingContext">The location of the objects in the directory.</param>
        /// <param name="objectClasses">The object classes for the object.</param>
        /// <param name="includeObjectClasses">Indicates if the object classes should be included in all queries.</param>
        /// <returns></returns>
        public LdapConfiguration AddMapping(IClassMap classMap, string namingContext = null, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true, string objectCategory = null, bool includeObjectCategory = true)
        {
            Mapper.Map(classMap, namingContext, objectClasses, includeObjectClasses, objectCategory, includeObjectCategory);
            
            return this;
        }

        /// <summary>
        /// Adds all mappings in the assembly.
        /// </summary>
        /// <param name="assembly">
        /// The assembly containing all of the mappings
        /// </param>
        /// <returns></returns>
        public LdapConfiguration AddMappingsFrom(Assembly assembly)
        {
            Mapper.AddMappingsFrom(assembly);

            return this;
        }

        /// <summary>
        /// Adds all mappings in the assembly.
        /// </summary>
        /// <param name="assemblyName">
        /// The assembly containing all of the mappings
        /// </param>
        /// <returns></returns>
        public LdapConfiguration AddMappingsFrom(string assemblyName)
        {
            Mapper.AddMappingsFrom(assemblyName);

            return this;
        }

        /// <summary>
        /// Starts configuration of <see cref="LdapConnectionFactory"/>.
        /// </summary>
        /// <param name="serverName">
        /// The server name can be an IP address, a DNS domain or host name.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// THrown if <paramref name="serverName"/> is null, empty or white-space.
        /// </exception>
        public ILdapConnectionFactoryConfiguration ConfigureFactory(string serverName)
        {
            if (ConnectionFactory != null)
                throw new MappingException("A connection factory has already been configured.");

            var factory = new LdapConnectionFactory(serverName) { Logger = Log };

            ConnectionFactory = factory;

            return factory;
        }

        /// <summary>
        /// Starts configuration of a connection factory that uses connection pooling.
        /// </summary>
        /// <param name="serverName">
        /// The server name can be an IP address, a DNS domain or host name.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// THrown if <paramref name="serverName"/> is null, empty or white-space.
        /// </exception>
        public IPooledConnectionFactoryConfiguration ConfigurePooledFactory(string serverName)
        {
            if (ConnectionFactory != null)
                throw new MappingException("A connection factory has already been configured.");

            var factory = new PooledLdapConnectionFactory(serverName){Logger = Log};

            ConnectionFactory = factory;

            return factory;
        }

        /// <summary>
        /// Configure a custom connection factory.
        /// </summary>
        /// <param name="customFactory">The factory.</param>
        /// <typeparam name="T">Type of the factory</typeparam>
        /// <returns></returns>
        public T ConfigureCustomFactory<T>(T customFactory) where T : ILdapConnectionFactory
        {
            if (ConnectionFactory != null) 
                throw new MappingException("A connection factory has already been configured.");

            ConnectionFactory = customFactory;
            
            return customFactory;
        }

        /// <summary>
        /// Sets <see cref="Configuration"/> for global configuration access.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="MappingException">Throw if this method is called more than once.</exception>
        public LdapConfiguration UseStaticStorage()
        {
            if (Configuration != null)
            {
                throw new MappingException("Static storage has already been set.");
            }
            Configuration = this;

            return this;
        }

        /// <summary>
        /// Creates a <see cref="DirectoryContext"/> from the configuration.
        /// </summary>
        ///<returns></returns>
        public IDirectoryContext CreateContext()
        {
            return new DirectoryContext(this);
        }

        /// <summary>
        /// Sets the logger for <see cref="ConnectionFactory"/> and <see cref="DirectoryContext"/>s created or configured by this class.
        /// </summary>
        /// <param name="log">The log implementation.</param>
        /// <returns></returns>
        public LdapConfiguration LogTo(ILinqToLdapLogger log)
        {
            Log = log;
            
            if (ConnectionFactory is ConnectionFactoryBase)
            {
                (ConnectionFactory as ConnectionFactoryBase).Logger = Log;
            }

            return this;
        }
    }
}
