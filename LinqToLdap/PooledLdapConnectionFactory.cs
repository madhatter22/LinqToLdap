using LinqToLdap.Helpers;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Net;
using System.Timers;

namespace LinqToLdap
{
    internal class PooledLdapConnectionFactory : ConnectionFactoryBase, IPooledConnectionFactoryConfiguration, IPooledLdapConnectionFactory, IDisposable
    {
        private bool _disposed;
        private object _connectionLockObject = new object();
        private Dictionary<LdapConnection, TwoTuple<DateTime, DateTime>> _availableConnections = new Dictionary<LdapConnection, TwoTuple<DateTime, DateTime>>();
        private Dictionary<LdapConnection, DateTime> _inUseConnections = new Dictionary<LdapConnection, DateTime>();
        private int _maxPoolSize = 50;
        private int _minPoolSize;
        private double _connectionIdleTime = 1;
        private Timer _timer;
        private bool _isFirstRequest = true;
        private TimeSpan _maxConnectionAge = TimeSpan.FromMinutes(30);

        public PooledLdapConnectionFactory(string serverName) : base(serverName)
        {
            _timer = new Timer();
            _timer.Elapsed += TimerElapsed;
            _timer.Interval = 90000;
        }

        ~PooledLdapConnectionFactory()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;

            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            var lockObject = _connectionLockObject;
            if (lockObject != null && disposing)
            {
                lock (lockObject)
                {
                    DisposeWork(disposing);
                }
            }
            else
            {
                DisposeWork(disposing);
            }

            Logger = null;
            _connectionLockObject = lockObject = null;
            Credentials = null;
        }

        private void DisposeWork(bool disposing)
        {
            var logger = Logger;
            try
            {
                var availableConnections = _availableConnections;
                var inUseConnections = _inUseConnections;
                var timer = _timer;

                if (timer != null)
                {
                    if (disposing)
                    {
                        timer.Stop();
                        timer.Elapsed -= TimerElapsed;
                        timer.Dispose();
                    }
                    _timer = timer = null;
                }

                if (availableConnections != null)
                {
                    foreach (var connection in availableConnections)
                    {
                        try
                        {
                            if (disposing) connection.Key.Dispose();
                        }
                        catch (Exception ex)
                        {
                            if (logger != null) logger.Error(ex);
                        }
                    }

                    availableConnections.Clear();
                    _availableConnections = availableConnections = null;
                }

                if (inUseConnections != null)
                {
                    foreach (var connection in inUseConnections)
                    {
                        try
                        {
                            if (disposing) connection.Key.Dispose();
                        }
                        catch (Exception ex)
                        {
                            if (logger != null)
                            {
                                logger.Error(ex);
                            }
                        }
                    }

                    inUseConnections.Clear();
                    _inUseConnections = inUseConnections = null;
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (logger != null) logger.Error(ex);
                }
                catch (Exception)
                {
                    //dispose should never throw an exception
                }
            }
            Logger = logger = null;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.ProtocolVersion(int version)
        {
            LdapProtocolVersion = version;
            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.UsePort(int port)
        {
            UsesSsl = false;
            Port = port;
            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.UseSsl(int port)
        {
            UsesSsl = true;
            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.UseSsl()
        {
            UsesSsl = true;
            Port = SslPort;
            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.ConnectionTimeoutIn(double seconds)
        {
            if (seconds <= 0) throw new ArgumentException("seconds must be greater than 0");
            Timeout = TimeSpan.FromSeconds(seconds);

            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.ServerNameIsFullyQualified()
        {
            FullyQualifiedDnsHostName = true;
            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.UseUdp()
        {
            IsConnectionless = true;
            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.AuthenticateBy(AuthType authType)
        {
            AuthType = authType;
            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.AuthenticateAs(NetworkCredential credentials)
        {
            Credentials = credentials;
            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.MaxPoolSizeIs(int size)
        {
            if (size < 1) throw new ArgumentException("MaxPoolSize must be greater than zero.");
            _maxPoolSize = size;
            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.MinPoolSizeIs(int size)
        {
            if (size < 0) throw new ArgumentException("MinPoolSize cannot be negative.");
            _minPoolSize = size;
            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.MaxConnectionAgeIs(TimeSpan timeSpan)
        {
            _maxConnectionAge = timeSpan;
            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.ConnectionIdleTimeIs(double idleTime)
        {
            if (idleTime < 0) throw new ArgumentException("ConnectionIdleTime cannot be negative.");
            _connectionIdleTime = idleTime;
            return this;
        }

        IPooledConnectionFactoryConfiguration IPooledConnectionFactoryConfiguration.ScavengeIntervalIs(double interval)
        {
            if (interval < 0) throw new ArgumentException("ScavengeInterval cannot be negative.");
            _timer.Interval = interval;
            return this;
        }

        public void ReleaseConnection(LdapConnection connection)
        {
            var lockObject = _connectionLockObject;
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);

            if (lockObject == null)
            {
                //lock is null so this class is being finalized
                connection.Dispose();
                _inUseConnections?.Remove(connection);
                if (Logger != null && Logger.TraceEnabled) Logger.Trace("Synchronization object lost. Disposing of connection.");
                return;
            }

            lock (lockObject)
            {
                DateTime createdDate;
                if (_inUseConnections.TryGetValue(connection, out createdDate))
                {
                    _inUseConnections.Remove(connection);
                    if (DateTime.Now.Subtract(createdDate).TotalMinutes < _maxConnectionAge.TotalMinutes)
                    {
                        _availableConnections.Add(connection, new TwoTuple<DateTime, DateTime>(createdDate, DateTime.Now));
                        if (Logger != null && Logger.TraceEnabled) Logger.Trace("Connection Marked As Available");
                    }
                    else
                    {
                        connection.Dispose();
                        if (Logger != null && Logger.TraceEnabled) Logger.Trace("Connection Exceeds Max Age. Connection Disposed.");
                    }
                }
                else
                {
                    connection.Dispose();
                    if (Logger != null && Logger.TraceEnabled) Logger.Trace("Connection Disposed");
                }
            }
        }

        public LdapConnection GetConnection()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
            LdapConnection connection;
            lock (_connectionLockObject)
            {
                try
                {
                    if (_isFirstRequest) InitializePool();

                    var pair = _availableConnections.FirstOrDefault();

                    while (true)
                    {
                        if (!Equals(pair, default(KeyValuePair<LdapConnection, TwoTuple<DateTime, DateTime>>)) && DateTime.Now.Subtract(pair.Value.Item1).TotalMinutes > _maxConnectionAge.TotalMinutes)
                        {
                            _availableConnections.Remove(pair.Key);
                            pair.Key.Dispose();
                            pair = _availableConnections.FirstOrDefault();
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (Equals(pair, default(KeyValuePair<LdapConnection, TwoTuple<DateTime, DateTime>>)))
                    {
                        if (Logger != null && Logger.TraceEnabled) Logger.Trace("Creating Connection For Use.");
                        if ((_inUseConnections.Count + _availableConnections.Count + 1) > _maxPoolSize)
                            throw new InvalidOperationException(
                                string.Format("LdapConnection pool limit of {0} exceeded.", _maxPoolSize));

                        connection = BuildConnection();
                        _inUseConnections.Add(connection, DateTime.Now);
                    }
                    else
                    {
                        if (Logger != null && Logger.TraceEnabled) Logger.Trace("Using Available Connection.");
                        _inUseConnections.Add(pair.Key, pair.Value.Item1);
                        _availableConnections.Remove(pair.Key);
                        connection = pair.Key;
                    }
                    if (Logger != null && Logger.TraceEnabled)
                    {
                        Logger.Trace("In Use Connection Count: " + _inUseConnections.Count);
                        Logger.Trace("Available Connection Count: " + _availableConnections.Count);
                    }
                }
                catch (Exception ex)
                {
                    if (Logger != null) Logger.Error(ex);
                    throw;
                }
            }
            return connection;
        }

        private void InitializePool()
        {
            if (Logger != null && Logger.TraceEnabled) Logger.Trace("Initializing Connection Pool.");
            for (int i = 0; i < _minPoolSize; i++)
            {
                _availableConnections.Add(BuildConnection(), new TwoTuple<DateTime, DateTime>(DateTime.Now, DateTime.Now));
            }

            _isFirstRequest = false;
            _timer.Start();
            if (Logger != null && Logger.TraceEnabled) Logger.Trace("Scavenge Timer Started.");
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
            lock (_connectionLockObject)
            {
                try
                {
                    if (Logger != null && Logger.TraceEnabled)
                    {
                        Logger.Trace("Available Connections Before Scavenge: " + _availableConnections.Count);
                        Logger.Trace("Scavenging Connections.");
                    }

                    ScavengeConnections(e.SignalTime);

                    if (Logger != null && Logger.TraceEnabled) Logger.Trace("Available Connections After Scavenge: " + _availableConnections.Count);
                }
                catch (Exception ex)
                {
                    if (Logger != null) Logger.Error(ex);
                    throw;
                }
            }
        }

        private void ScavengeConnections(DateTime signalTime)
        {
            int amountToScavenge = _minPoolSize == 0
                                               ? _availableConnections.Count
                                               : (_availableConnections.Count - _minPoolSize);

            if (amountToScavenge <= 0) return;

            var expiredConnections = (from pair in _availableConnections
                                      where signalTime.Subtract(pair.Value.Item2).TotalMinutes > _connectionIdleTime
                                      select pair.Key).ToList();

            foreach (var expiredConnection in expiredConnections)
            {
                if (amountToScavenge == 0) break;

                _availableConnections.Remove(expiredConnection);
                expiredConnection.Dispose();
                if (Logger != null && Logger.TraceEnabled) Logger.Trace("Connection Scavenged.");
                amountToScavenge--;
            }
        }

        public void Reinitialize()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
            lock (_connectionLockObject)
            {
                if (_timer.Enabled)
                {
                    _timer.Stop();
                    if (Logger != null && Logger.TraceEnabled) Logger.Trace("Scavenge Timer Stopped.");
                }
                if (Logger != null && Logger.TraceEnabled) Logger.Trace("Initializing Connection Pool.");

                //LdapConnection has a finalizer so once the connections fall out of scope or
                //DirectoryContext explicitly calls ReleaseConnection, they will be cleaned up.
                _inUseConnections.Clear();

                foreach (var availableConnection in _availableConnections)
                {
                    availableConnection.Key.Dispose();
                }
                _availableConnections.Clear();

                _isFirstRequest = true;
            }
        }
    }
}