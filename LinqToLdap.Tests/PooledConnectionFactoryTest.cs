using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Timers;
using LinqToLdap.Helpers;
using LinqToLdap.Logging;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;

namespace LinqToLdap.Tests
{
    [TestClass]
    public class PooledConnectionFactoryTest
    {
        private PooledLdapConnectionFactory _factory;

        [TestCleanup]
        public void TearDown()
        {
            _factory?.Dispose();
        }

        [TestMethod]
        public void GetConnection_FirstTimeWithMinPoolSize_InitializesConnectionPool()
        {
            //prepare
            _factory = new PooledLdapConnectionFactory("localhost");
            _factory.CastTo<IPooledConnectionFactoryConfiguration>().MinPoolSizeIs(2);

            //act
            _factory.GetConnection();

            //assert
            _factory.FieldValueEx<Dictionary<LdapConnection, TwoTuple<DateTime, DateTime>>>("_availableConnections")
                .Should().HaveCount(1);
            _factory.FieldValueEx<Dictionary<LdapConnection, DateTime>>("_inUseConnections")
                .Should().HaveCount(1);
        }

        [TestMethod]
        public void GetConnection_FirstTimeWithoutMinPoolSize_InitializesConnectionPool()
        {
            //act
            _factory = new PooledLdapConnectionFactory("localhost");
            _factory.GetConnection();

            //assert
            _factory.FieldValueEx<Dictionary<LdapConnection, TwoTuple<DateTime, DateTime>>>("_availableConnections")
                .Should().HaveCount(0);
            _factory.FieldValueEx<Dictionary<LdapConnection, DateTime>>("_inUseConnections")
                .Should().HaveCount(1);
        }

        [TestMethod]
        public void ReleaseConnection_InUseConnection_RemovesFromInUseAndAddsToAvailable()
        {
            //prepare
            _factory = new PooledLdapConnectionFactory("localhost");
            var connection = _factory.GetConnection();

            //act
            _factory.ReleaseConnection(connection);

            //assert
            _factory.FieldValueEx<Dictionary<LdapConnection, TwoTuple<DateTime, DateTime>>>("_availableConnections")
                .Should().HaveCount(1);
            _factory.FieldValueEx<Dictionary<LdapConnection, DateTime>>("_inUseConnections")
                .Should().HaveCount(0);
        }

        [TestMethod]
        public void GetConnection_MaxAge_Exceeded_DisposesConnections()
        {
            //prepare
            _factory = new PooledLdapConnectionFactory("localhost");
            _factory.CastTo<IPooledConnectionFactoryConfiguration>().ScavengeIntervalIs(1000).MaxConnectionAgeIs(TimeSpan.FromMilliseconds(1000));
            var connection = _factory.GetConnection();
            connection.Should().NotBeNull();
            _factory.ReleaseConnection(connection);
            var same = _factory.GetConnection();
            same.Should().BeSameAs(connection);
            _factory.ReleaseConnection(same);

            System.Threading.Thread.Sleep(1000);

            //act
            var connection2 = _factory.GetConnection();

            //assert
            connection2.Should().NotBeSameAs(connection);
            _factory.ReleaseConnection(connection2);
        }

        [TestMethod]
        public void ReleaseConnection_UnknownConnection_CallsDispose()
        {
            //prepare
            _factory = new PooledLdapConnectionFactory("localhost");
            var connection = new LdapConnection("localhost");

            //act
            _factory.ReleaseConnection(connection);

            //assert
            connection.FieldValueEx<bool>("_disposed").Should().BeTrue();
            _factory.FieldValueEx<Dictionary<LdapConnection, TwoTuple<DateTime, DateTime>>>("_availableConnections")
                .Should().HaveCount(0);
            _factory.FieldValueEx<Dictionary<LdapConnection, DateTime>>("_inUseConnections")
                .Should().HaveCount(0);
        }

        [TestMethod]
        public void ReleaseConnection_Null_LockObject_Disposes_Of_Connection_And_Removes_It_From__InUseCollections()
        {
            //prepare
            _factory = new PooledLdapConnectionFactory("localhost");
            var connection = _factory.GetConnection();
            _factory.FieldValueEx<Dictionary<LdapConnection, DateTime>>("_inUseConnections")
                .Should().HaveCount(1);
            _factory.SetFieldValue<object>("_connectionLockObject", null);

            //act
            _factory.ReleaseConnection(connection);

            //assert
            _factory.FieldValueEx<Dictionary<LdapConnection, TwoTuple<DateTime, DateTime>>>("_availableConnections")
                .Should().HaveCount(0);
        }

        [TestMethod]
        public void ReInitializePool_Disposed_ThrowsException()
        {
            //prepare
            _factory = new PooledLdapConnectionFactory("localhost");
            _factory.Dispose();

            //assert
            Action work = () => _factory.Reinitialize();

            work.Should().Throw<ObjectDisposedException>()
                .And.ObjectName.Should().Be(_factory.GetType().FullName);
        }

        [TestMethod]
        public void ReInitializePool_NotDispose_ReInitializes()
        {
            //prepare
            _factory = new PooledLdapConnectionFactory("localhost");
            _factory.CastTo<IPooledConnectionFactoryConfiguration>().MinPoolSizeIs(2);
            var logger = new Mock<ILinqToLdapLogger>();
            logger.SetupGet(l => l.TraceEnabled)
                  .Returns(true);
            _factory.Logger = logger.Object;
            _factory.GetConnection();

            //act
            _factory.Reinitialize();

            //assert
            logger.Verify(l => l.Trace("Scavenge Timer Stopped."), Times.Once());
            logger.Verify(l => l.Trace("Initializing Connection Pool."), Times.Exactly(2));
            _factory.FieldValueEx<Timer>("_timer").Enabled.Should().BeFalse();
            _factory.FieldValueEx<bool>("_isFirstRequest").Should().BeTrue();
            _factory.FieldValueEx<Dictionary<LdapConnection, TwoTuple<DateTime, DateTime>>>("_availableConnections")
                .Should().HaveCount(0);
            _factory.FieldValueEx<Dictionary<LdapConnection, DateTime>>("_inUseConnections")
                .Should().HaveCount(0);
        }
    }
}