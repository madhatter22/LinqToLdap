using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Timers;
using LinqToLdap.Logging;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpTestsEx;

namespace LinqToLdap.Tests
{
    [TestClass]
    public class PooledConnectionFactoryTest
    {
        private PooledLdapConnectionFactory _factory;

        [TestInitialize]
        public void SetUp()
        {
            _factory = new PooledLdapConnectionFactory("localhost");
        }

        [TestCleanup]
        public void TearDown()
        {
            _factory.Dispose();
        }

        [TestMethod]
        public void GetConnection_FirstTimeWithMinPoolSize_InitializesConnectionPool()
        {
            //prepare
            _factory.As<IPooledConnectionFactoryConfiguration>().MinPoolSizeIs(2);

            //act
             _factory.GetConnection();

            //assert
            _factory.FieldValueEx<Dictionary<LdapConnection, DateTime>>("_availableConnections")
                .Should().Have.Count.EqualTo(1);
            _factory.FieldValueEx<List<LdapConnection>>("_inUseConnections")
                .Should().Have.Count.EqualTo(1);
        }

        [TestMethod]
        public void GetConnection_FirstTimeWithoutMinPoolSize_InitializesConnectionPool()
        {
            //act
            _factory.GetConnection();

            //assert
            _factory.FieldValueEx<Dictionary<LdapConnection, DateTime>>("_availableConnections")
                .Should().Have.Count.EqualTo(0);
            _factory.FieldValueEx<List<LdapConnection>>("_inUseConnections")
                .Should().Have.Count.EqualTo(1);
        }

        [TestMethod]
        public void ReleaseConnection_InUseConnection_RemovesFromInUseAndAddsToAvailable()
        {
            //prepare
            var connection = _factory.GetConnection();

            //act
            _factory.ReleaseConnection(connection);

            //assert
            _factory.FieldValueEx<Dictionary<LdapConnection, DateTime>>("_availableConnections")
                .Should().Have.Count.EqualTo(1);
            _factory.FieldValueEx<List<LdapConnection>>("_inUseConnections")
                .Should().Have.Count.EqualTo(0);
        }

        [TestMethod]
        public void ReleaseConnection_UnknownConnection_CallsDispose()
        {
            //prepare
            var connection = new LdapConnection("localhost");

            //act
            _factory.ReleaseConnection(connection);

            //assert
            connection.FieldValueEx<bool>("disposed").Should().Be.True();
            _factory.FieldValueEx<Dictionary<LdapConnection, DateTime>>("_availableConnections")
                .Should().Have.Count.EqualTo(0);
            _factory.FieldValueEx<List<LdapConnection>>("_inUseConnections")
                .Should().Have.Count.EqualTo(0);
        }

        [TestMethod]
        public void ReInitializePool_Disposed_ThrowsException()
        {
            //prepare
            _factory.Dispose();

            //assert
            Action work = () => _factory.Reinitialize();

            work.Should().Throw<ObjectDisposedException>()
                .And.Exception.ObjectName.Should().Be.EqualTo(_factory.GetType().FullName);
        }

        [TestMethod]
        public void ReInitializePool_NotDispose_ReInitializes()
        {
            //prepare
            _factory.As<IPooledConnectionFactoryConfiguration>().MinPoolSizeIs(2);
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
            _factory.FieldValueEx<Timer>("_timer").Enabled.Should().Be.False();
            _factory.FieldValueEx<bool>("_isFirstRequest").Should().Be.True();
            _factory.FieldValueEx<Dictionary<LdapConnection, DateTime>>("_availableConnections")
                .Should().Have.Count.EqualTo(0);
            _factory.FieldValueEx<List<LdapConnection>>("_inUseConnections")
                .Should().Have.Count.EqualTo(0);
        }
    }
}