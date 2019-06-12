using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Xml;
using LinqToLdap.Collections;
using LinqToLdap.EventListeners;
using LinqToLdap.Exceptions;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.TestSupport;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SharpTestsEx;

namespace LinqToLdap.Tests
{
    [TestClass]
    public class DirectoryContextTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_RawConnection_CreatesInternalConfiguration()
        {
            //prepare
            var connection = new MockLdapConnection();

            //act
            var context = new DirectoryContext(connection);

            //assert
            context.FieldValueEx<bool>("_disposeOfConnection").Should().Be.False();
            context.FieldValueEx<ILdapConfiguration>("_configuration").Should().Not.Be.Null();
        }

        [TestMethod]
        public void Constructor_NullConnection_ThrowsArgumentNullException()
        {
            //assert
            Executing.This(() => new DirectoryContext(default(LdapConnection))).Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Constructor_NullConfiguration_ThrowsMappingException()
        {
            //assert
            Executing.This(() => new DirectoryContext(default(ILdapConfiguration))).Should()
                .Throw<MappingException>().And.Exception.Message
                .Should().Be.EqualTo("configuration cannot be null and must be associated with a connection factory.");
        }

        [TestMethod]
        public void Constructor_ConfigurationWithoutConnectionFactory_ThrowsMappingException()
        {
            //prepare
            var config = new Mock<ILdapConfiguration>();
            config.SetupGet(c => c.ConnectionFactory)
                .Returns(default(ILdapConnectionFactory));

            //assert
            Executing.This(() => new DirectoryContext(config.Object)).Should()
                .Throw<MappingException>().And.Exception.Message
                .Should().Be.EqualTo("configuration cannot be null and must be associated with a connection factory.");
        }

        [TestMethod]
        public void Constructor_DefaultWithoutStaticConfiguration_ThrowsMappingException()
        {
            //assert
            Executing.This(() => new DirectoryContext()).Should()
                .Throw<MappingException>().And.Exception.Message
                .Should().Be.EqualTo("A static configuration and connection factory must be provided. See LdapConfiguration.UseStaticStorage()");
        }

        #endregion Constructor Tests

        #region Update Tests

        [TestMethod]
        public void UpdateGeneric_Disposed_ThrowsObjectDisposedException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var context = new DirectoryContext(connection);

            //act
            context.Dispose();

            //assert
            Executing.This(() => context.Update(this)).Should().Throw<ObjectDisposedException>();
        }

        [TestMethod]
        public void UpdateGeneric_NullEntry_ThrowsArgumentNullException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var context = new DirectoryContext(connection);

            //assert
            Executing.This(() => context.Update(default(object))).Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void UpdateGeneric_NullEntryWithLog_ThrowsArgumentNullExceptionAndLogsIt()
        {
            //prepare
            var connection = new MockLdapConnection();
            var context = new DirectoryContext(connection);
            var log = new Mock<ILinqToLdapLogger>();
            context.Logger = log.Object;

            //assert
            Executing.This(() => context.Update(default(object))).Should().Throw<ArgumentNullException>();
            log.Verify(l => l.Error(It.IsAny<ArgumentNullException>(), "An error occurred while trying to update ''."));
        }

        [TestMethod]
        public void UpdateGeneric_NoObjectMapping_ThrowsMappingException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var mapper = new Mock<IDirectoryMapper>();

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            mapper.Setup(m => m.GetMapping<string>())
                .Returns(default(IObjectMapping));

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.Update("test"))
                .Should().Throw<MappingException>()
                .And.Exception.Message.Should().Be.EqualTo("Cannot update an unmapped class.");
        }

        [TestMethod]
        public void UpdateGeneric_WithEventListeners_CallsListenersEvent()
        {
            //prepare
            var obj = new object();
            object[] parameters;
#if (!NET35 && !NET40 && !NET45)
            parameters = new object[] { "", new System.DirectoryServices.Protocols.DirectoryControl[0], System.DirectoryServices.Protocols.ResultCode.Success, "", new System.Uri[0] };
#else
            parameters = new object[] { new XmlDocument() };
#endif
            var response = typeof(ModifyResponse).Create<ModifyResponse>(parameters);
#if (NET35 || NET40 || NET45)
            response.SetFieldValue("result", ResultCode.Success);
#endif
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyRequest), response } });
            var configuration = new Mock<ILdapConfiguration>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();
            var propertyMapping1 = new Mock<IPropertyMapping>();
            propertyMapping1.Setup(pm => pm.GetDirectoryAttributeModification(obj))
                .Returns("pm1".ToDirectoryModification("att1", DirectoryAttributeOperation.Replace));

            objectMapping.Setup(om => om.GetUpdateablePropertyMappings())
                .Returns(new List<IPropertyMapping> { propertyMapping1.Object });

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            mapper.Setup(m => m.GetMapping<object>())
                .Returns(objectMapping.Object);

            var preUpdate = new Mock<IPreUpdateEventListener>();
            var postUpdate = new Mock<IPostUpdateEventListener>();

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            configuration.Setup(c => c.GetListeners<IPreUpdateEventListener>())
                .Returns(new[] { preUpdate.Object });

            configuration.Setup(c => c.GetListeners<IPostUpdateEventListener>())
                .Returns(new[] { postUpdate.Object });

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.Update(obj, "dn");

            //assert
            preUpdate.Verify(el => el.Notify(It.IsAny<ListenerPreArgs<object, ModifyRequest>>()));
            postUpdate.Verify(el => el.Notify(It.Is<ListenerPostArgs<object, ModifyRequest, ModifyResponse>>(a => a.Request.Modifications.Count > 0)));
        }

        [TestMethod]
        public void UpdateGeneric_NoModifications_DoesNotSendRequest()
        {
            //prepare
            var obj = new object();
            var updateResponse = typeof(ModifyResponse).Create<ModifyResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), updateResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();

            objectMapping.Setup(om => om.GetUpdateablePropertyMappings())
                .Returns(new List<IPropertyMapping>());

            objectMapping.SetupGet(om => om.ObjectClasses)
                .Returns(new[] { "oc" });

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.Update(obj, "dn");

            //assert
            connection.SentRequests.Should().Be.Empty();
        }

        [TestMethod]
        public void UpdateGeneric_NoModificationsWithLogTraceDisabled_DoesNotLogIt()
        {
            //prepare
            var obj = new object();
            var updateResponse = typeof(ModifyResponse).Create<ModifyResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), updateResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();

            objectMapping.Setup(om => om.GetUpdateablePropertyMappings())
                .Returns(new List<IPropertyMapping>());

            objectMapping.SetupGet(om => om.ObjectClasses)
                .Returns(new[] { "oc" });

            log.SetupGet(l => l.TraceEnabled)
                .Returns(false);

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.Update(obj, "dn");

            //assert
            log.Verify(l => l.TraceEnabled, Times.Once());
            log.Verify(l => l.Trace("No changes found for dn."), Times.Never());
        }

        [TestMethod]
        public void UpdateGeneric_NoModificationsWithLogTraceEnabled_LogIt()
        {
            //prepare
            var obj = new object();
            var updateResponse = typeof(ModifyResponse).Create<ModifyResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), updateResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();

            objectMapping.Setup(om => om.GetUpdateablePropertyMappings())
                .Returns(new List<IPropertyMapping>());

            objectMapping.SetupGet(om => om.ObjectClasses)
                .Returns(new[] { "oc" });

            log.SetupGet(l => l.TraceEnabled)
                .Returns(true);

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.Update(obj, "dn");

            //assert
            log.Verify(l => l.TraceEnabled, Times.Once());
            log.Verify(l => l.Trace("No changes found for dn."), Times.Once());
        }

        [TestMethod]
        public void UpdateAndGetGeneric_NonIDirectoryObjectAndControls_SendsRequest()
        {
            //prepare
            var controls = new DirectoryControl[] { new AsqRequestControl("a") };
            var obj = new object();
            var modifyResponse = typeof(ModifyResponse).Create<ModifyResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyRequest), modifyResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();
            var propertyMapping1 = new Mock<IPropertyMapping>();
            propertyMapping1.Setup(pm => pm.GetDirectoryAttributeModification(obj))
                .Returns("pm1".ToDirectoryModification("att1", DirectoryAttributeOperation.Replace));

            objectMapping.Setup(om => om.GetUpdateablePropertyMappings())
                .Returns(new List<IPropertyMapping> { propertyMapping1.Object });

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN<object>(connection, objectMapping, mapper);

            //act
            context.UpdateAndGet(obj, "dn", controls);

            //assert
            configuration.VerifyAll();
            mapper.VerifyAll();
            objectMapping.VerifyAll();
            propertyMapping1.VerifyAll();

            var modifyRequest = connection.SentRequests[0].CastTo<ModifyRequest>();
            modifyRequest.Modifications.Count.Should().Be.EqualTo(1);
            modifyRequest.Modifications[0].Name.Should().Be.EqualTo("att1");
            modifyRequest.Modifications[0][0].Should().Be.EqualTo("pm1");
            modifyRequest.DistinguishedName.Should().Be.EqualTo("dn");
            modifyRequest.Controls.Cast<DirectoryControl>().Should().Have.SameSequenceAs(controls);
        }

        [TestMethod]
        public void UpdateGeneric_ValidRequestFailedResponse_ThrowsLdapException()
        {
            //prepare
            var modifyResponse = typeof(ModifyResponse).Create<ModifyResponse>("dn", new DirectoryControl[0], ResultCode.Unavailable,
                                                                       "boop!", new Uri[0]);
            var obj = new object();
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyRequest), modifyResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();
            var propertyMapping1 = new Mock<IPropertyMapping>();
            propertyMapping1.Setup(pm => pm.GetDirectoryAttributeModification(obj))
                .Returns("pm1".ToDirectoryModification("att1", DirectoryAttributeOperation.Replace));

            objectMapping.Setup(om => om.GetUpdateablePropertyMappings())
                .Returns(new List<IPropertyMapping> { propertyMapping1.Object });

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN<object>(connection, objectMapping, mapper);

            //assert
            Executing.This(() => context.Update(obj, "dn"))
                .Should().Throw<LdapException>().And.Exception.Message
                .Should().Be.EqualTo(modifyResponse.ToLogString());
        }

        [TestMethod]
        public void UpdateAndGetGeneric_NonIDirectoryObjectAndControlsWithLogTraceDisabled_SendsRequestAndDoesNotLogIt()
        {
            //prepare
            var obj = new object();
            var controls = new DirectoryControl[] { new AsqRequestControl("a") };
            var modifyResponse = typeof(ModifyResponse).Create<ModifyResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyRequest), modifyResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();
            var propertyMapping1 = new Mock<IPropertyMapping>();
            propertyMapping1.Setup(pm => pm.GetDirectoryAttributeModification(obj))
                .Returns("pm1".ToDirectoryModification("att1", DirectoryAttributeOperation.Replace));

            objectMapping.Setup(om => om.GetUpdateablePropertyMappings())
                .Returns(new List<IPropertyMapping> { propertyMapping1.Object });

            log.SetupGet(l => l.TraceEnabled)
                .Returns(false);

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN<object>(connection, objectMapping, mapper);

            //act
            context.UpdateAndGet(obj, "dn", controls);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Exactly(2));
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void UpdateAndGetGeneric_NonIDirectoryObjectAndControlsWithLogTraceEnabled_SendsRequestAndLogIt()
        {
            //prepare
            var controls = new DirectoryControl[] { new AsqRequestControl("a") };
            var obj = new object();
            var modifyResponse = typeof(ModifyResponse).Create<ModifyResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyRequest), modifyResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();
            var propertyMapping1 = new Mock<IPropertyMapping>();
            propertyMapping1.Setup(pm => pm.GetDirectoryAttributeModification(obj))
                .Returns("pm1".ToDirectoryModification("att1", DirectoryAttributeOperation.Replace));

            objectMapping.Setup(om => om.GetUpdateablePropertyMappings())
                .Returns(new List<IPropertyMapping> { propertyMapping1.Object });

            log.SetupGet(l => l.TraceEnabled)
                .Returns(true);

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN<object>(connection, objectMapping, mapper);

            //act
            context.UpdateAndGet(obj, "dn", controls);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Exactly(2));
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void UpdateAndGetGeneric_IDirectoryObject_SendsRequest()
        {
            //prepare
            var modifyResponse = typeof(ModifyResponse).Create<ModifyResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);

            var directoryObject = new Mock<IDirectoryObject>();
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyRequest), modifyResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            mapper.Setup(m => m.GetMapping(directoryObject.Object.GetType()))
                .Returns(objectMapping.Object);

            directoryObject.Setup(d => d.GetChanges(objectMapping.Object))
                .Returns(new[] { "pm1".ToDirectoryModification("att1", DirectoryAttributeOperation.Replace) });

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN<IDirectoryObject>(connection, objectMapping, mapper);

            //act
            context.UpdateAndGet(directoryObject.Object, "dn");

            //assert
            connection.SentRequests.Count.Should().Be.EqualTo(2);
            connection.SentRequests[1]
                .Should().Be.InstanceOf<SearchRequest>().And.Value.DistinguishedName
                .Should().Be.EqualTo("dn");
            var modifyRequest = connection.SentRequests[0].CastTo<ModifyRequest>();
            modifyRequest.Modifications.Count.Should().Be.EqualTo(1);
            modifyRequest.Modifications[0].Name.Should().Be.EqualTo("att1");
            modifyRequest.Modifications[0][0].Should().Be.EqualTo("pm1");
            modifyRequest.DistinguishedName.Should().Be.EqualTo("dn");
        }

        [TestMethod]
        public void Update_NullEntry_ThrowsArgumentNullException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.Update(default(IDirectoryAttributes)))
                .Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Update_NullEntryWithLig_ThrowsArgumentNullExceptionAndLogsIt()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.Update(default(IDirectoryAttributes)))
                .Should().Throw<ArgumentNullException>();

            log.Verify(l => l.Error(It.IsAny<ArgumentNullException>(), "An error occurred while trying to update ''."));
        }

        [TestMethod]
        public void Update_NullDistinguishedName_ThrowsArgumentException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var context = new DirectoryContext(connection, configuration: configuration.Object);
            var directoryAttributes = new DirectoryAttributes();

            //assert
            Executing.This(() => context.Update(directoryAttributes))
                .Should().Throw<ArgumentException>().And.Exception.Message
                .Should().Be.EqualTo("entry.DistinguishedName is invalid.");
        }

        [TestMethod]
        public void UpdateAndGet_NoChanges_ReturnsGetByDNResult()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var context = new DirectoryContext(connection, configuration: configuration.Object);
            var directoryAttributes = new DirectoryAttributes("dn");

            MockGetByDN(connection);

            //act
            context.UpdateAndGet(directoryAttributes);

            //assert
            connection.SentRequests.Should().Have.Count.EqualTo(1);
            connection.SentRequests.First().Should().Be.OfType<SearchRequest>()
                .And.Value.DistinguishedName.Should().Be.EqualTo("dn");
        }

        [TestMethod]
        public void UpdateAndGet_NoChangesWithLogTraceDisabled_DoesNotLogIt()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var directoryAttributes = new DirectoryAttributes("dn");
            var log = new Mock<ILinqToLdapLogger>();
            log.SetupGet(l => l.TraceEnabled)
                .Returns(false);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            MockGetByDN(connection);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.UpdateAndGet(directoryAttributes);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Exactly(2));
            log.Verify(l => l.Trace("No changes found for dn."), Times.Never());
        }

        [TestMethod]
        public void UpdateAndGet_NoChangesWithLogTraceEnabled_LogsIt()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var directoryAttributes = new DirectoryAttributes("dn");
            var log = new Mock<ILinqToLdapLogger>();
            log.SetupGet(l => l.TraceEnabled)
                .Returns(true);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            MockGetByDN(connection);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.UpdateAndGet(directoryAttributes);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Exactly(2));
            log.Verify(l => l.Trace("No changes found for dn."), Times.Once());
        }

        [TestMethod]
        public void Update_ValidRequestWithEventListener_RaisesEvent()
        {
            //prepare
            var preUpdateListener = new Mock<IPreUpdateEventListener>();
            var postUpdateListener = new Mock<IPostUpdateEventListener>();
            object[] parameters;
#if (!NET35 && !NET40 && !NET45)
            parameters = new object[] { "", new System.DirectoryServices.Protocols.DirectoryControl[0], System.DirectoryServices.Protocols.ResultCode.Success, "", new System.Uri[0] };
#else
            parameters = new object[] { new XmlDocument() };
#endif
            var response = typeof(ModifyResponse).Create<ModifyResponse>(parameters);
#if (NET35 || NET40 || NET45)
            response.SetFieldValue("result", ResultCode.Success);
#endif
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyRequest), response } });

            var configuration = new Mock<ILdapConfiguration>();

            configuration.Setup(c => c.GetListeners<IUpdateEventListener>())
                .Returns(new IUpdateEventListener[] { preUpdateListener.Object, postUpdateListener.Object });

            var directoryAttributes = new DirectoryAttributes("dn");
            directoryAttributes.Set("test", "value");

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.Update(directoryAttributes);

            //assert
            preUpdateListener.Verify(l => l.Notify(It.IsAny<ListenerPreArgs<object, ModifyRequest>>()), Times.Once());
            postUpdateListener.Verify(l => l.Notify(It.IsAny<ListenerPostArgs<object, ModifyRequest, ModifyResponse>>()), Times.Once());
        }

        [TestMethod]
        public void UpdateAndGet_ValidRequestWithControls_SendsRequest()
        {
            //prepare
            var controls = new DirectoryControl[] { new AsqRequestControl() };
            var modifyResponse = typeof(ModifyResponse).Create<ModifyResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyRequest), modifyResponse } });
            var configuration = new Mock<ILdapConfiguration>();

            var directoryAttributes = new DirectoryAttributes("dn");
            directoryAttributes.Set("test", "value");

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN(connection);

            //act
            context.UpdateAndGet(directoryAttributes, controls);

            //assert
            connection.SentRequests.Count.Should().Be.EqualTo(2);
            connection.SentRequests[1]
                .Should().Be.InstanceOf<SearchRequest>().And.Value.DistinguishedName
                .Should().Be.EqualTo("dn");
            var modifyRequest = connection.SentRequests[0].CastTo<ModifyRequest>();
            modifyRequest.Controls.OfType<DirectoryControl>().Should().Have.SameSequenceAs(controls);
            modifyRequest.Modifications.Count.Should().Be.EqualTo(1);
            modifyRequest.Modifications[0].Name.Should().Be.EqualTo("test");
            modifyRequest.Modifications[0][0].Should().Be.EqualTo("value");
            modifyRequest.DistinguishedName.Should().Be.EqualTo("dn");
        }

        [TestMethod]
        public void UpdateAndGet_ValidRequestFailedResponse_ThrowsLdapException()
        {
            //prepare
            var modifyResponse = typeof(ModifyResponse).Create<ModifyResponse>("dn", new DirectoryControl[0], ResultCode.Unavailable,
                                                                       "boop!", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyRequest), modifyResponse } });
            var configuration = new Mock<ILdapConfiguration>();

            var directoryAttributes = new DirectoryAttributes("dn");
            directoryAttributes.Set("test", "value");

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN(connection);

            //assert
            Executing.This(() => context.UpdateAndGet(directoryAttributes))
                .Should().Throw<LdapException>().And.Exception.Message
                .Should().Be.EqualTo(modifyResponse.ToLogString());
        }

        [TestMethod]
        public void UpdateAndGet_ValidRequestWithLogTraceDisabled_SendsRequestAndDoesNotLogIt()
        {
            //prepare

            var modifyResponse = typeof(ModifyResponse).Create<ModifyResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyRequest), modifyResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();

            log.SetupGet(l => l.TraceEnabled)
                .Returns(false);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var directoryAttributes = new DirectoryAttributes("dn");
            directoryAttributes.Set("test", "value");

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN(connection);

            //act
            context.UpdateAndGet(directoryAttributes);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Exactly(2));
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void UpdateAndGet_ValidRequestWithLogTraceEnabled_SendsRequestAndLogsIt()
        {
            //prepare
            var modifyResponse = typeof(ModifyResponse).Create<ModifyResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyRequest), modifyResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();

            log.SetupGet(l => l.TraceEnabled)
                .Returns(true);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var directoryAttributes = new DirectoryAttributes("dn");
            directoryAttributes.Set("test", "value");

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN(connection);

            //act
            context.UpdateAndGet(directoryAttributes);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Exactly(2));
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Exactly(2));
        }

        #endregion Update Tests

        #region Add Tests

        [TestMethod]
        public void Add_Disposed_ThrowsObjectDisposedException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var context = new DirectoryContext(connection);

            //act
            context.Dispose();

            //assert
            Executing.This(() => context.Add(new DirectoryAttributes("test"))).Should().Throw<ObjectDisposedException>();
        }

        [TestMethod]
        public void Add_NullEntry_ThrowsArgumentNullException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var context = new DirectoryContext(connection);

            //assert
            Executing.This(() => context.Add(default(IDirectoryAttributes))).Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void Add_NullEntryWithLog_ThrowsArgumentNullExceptionAndLogsIt()
        {
            //prepare
            var connection = new MockLdapConnection();
            var log = new Mock<ILinqToLdapLogger>();
            var context = new DirectoryContext(connection) { Logger = log.Object };

            //assert
            Executing.This(() => context.Add(default(IDirectoryAttributes))).Should().Throw<ArgumentNullException>();
            log.Verify(l => l.Error(It.IsAny<ArgumentNullException>(), "An error occurred while trying to add ''."), Times.Once());
        }

        [TestMethod]
        public void Add_WithPreAddEventListener_CallsListenersEvent()
        {
            //prepare
            var xml = new XmlDocument();
            object[] parameters;
#if (!NET35 && !NET40 && !NET45)
            parameters = new object[] { "", new System.DirectoryServices.Protocols.DirectoryControl[0], System.DirectoryServices.Protocols.ResultCode.Success, "", new System.Uri[0] };
#else
            parameters = new object[] { new XmlDocument() };
#endif
            var addResponse = typeof(AddResponse).Create<AddResponse>(parameters);
#if (NET35 || NET40 || NET45)
            addResponse.SetFieldValue("result", ResultCode.Success);
#endif
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), addResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var preAdd = new Mock<IPreAddEventListener>();
            var postAdd = new Mock<IPostAddEventListener>();
            var attributes = new DirectoryAttributes("dn")
                .Set("test", "value");
            configuration.Setup(c => c.GetListeners<IAddEventListener>())
                .Returns(new IAddEventListener[] { preAdd.Object, postAdd.Object });

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            context.Add(attributes);

            //assert
            preAdd.Verify(l => l.Notify(It.IsAny<ListenerPreArgs<object, AddRequest>>()), Times.Once());
            postAdd.Verify(l => l.Notify(It.IsAny<ListenerPostArgs<object, AddRequest, AddResponse>>()), Times.Once());
        }

        [TestMethod]
        public void Add_ValidRequest_SendsRequest()
        {
            //prepare
            var directoryAttributes = new DirectoryAttributes("dn", new List<KeyValuePair<string, object>>
                                                                        {
                                                                            new KeyValuePair<string, object>("att1", "pm1"),
                                                                            new KeyValuePair<string, object>("att2", null),
                                                                            new KeyValuePair<string, object>("objectClass", new[] {"oc"})
                                                                        });
            var controls = new DirectoryControl[] { new AsqRequestControl("a") };
            var addResponse = typeof(AddResponse).Create<AddResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), addResponse } });
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN(connection);

            //act
            context.Add(directoryAttributes, controls);

            //assert
            var addRequest = connection.SentRequests[0].CastTo<AddRequest>();
            addRequest.Attributes.Count.Should().Be.EqualTo(2);
            addRequest.Attributes[0].Name.Should().Be.EqualTo("att1");
            addRequest.Attributes[0][0].Should().Be.EqualTo("pm1");
            addRequest.Attributes[1].Name.Should().Be.EqualTo("objectClass");
            addRequest.Attributes[1][0].Should().Be.EqualTo("oc");
            addRequest.DistinguishedName.Should().Be.EqualTo("dn");
            addRequest.Controls.Cast<DirectoryControl>().Should().Have.SameSequenceAs(controls);
        }

        [TestMethod]
        public void Add_ValidRequestFailedResponse_ThrowsLdapException()
        {
            //prepare
            var directoryAttributes = new DirectoryAttributes("dn", new List<KeyValuePair<string, object>>
                                                                        {
                                                                            new KeyValuePair<string, object>("att1", "pm1"),
                                                                            new KeyValuePair<string, object>("att2", null),
                                                                            new KeyValuePair<string, object>("objectClass", new[] {"oc"})
                                                                        });
            var addResponse = typeof(AddResponse).Create<AddResponse>("dn", new DirectoryControl[0], ResultCode.StrongAuthRequired,
                                                                       "word!", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), addResponse } });
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN(connection);

            //assert
            Executing.This(() => context.Add(directoryAttributes))
                .Should().Throw<LdapException>().And.Exception.Message
                .Should().Be.EqualTo(addResponse.ToLogString());
        }

        [TestMethod]
        public void AddAndGet_ValidRequestWithLogTraceDisabled_SendsRequestAndDoesNotLogIt()
        {
            //prepare
            var directoryAttributes = new DirectoryAttributes("dn", new List<KeyValuePair<string, object>>
                                                                        {
                                                                            new KeyValuePair<string, object>("att1", "pm1"),
                                                                            new KeyValuePair<string, object>("att2", null),
                                                                            new KeyValuePair<string, object>("objectClass", new[] {"oc"})
                                                                        });
            var controls = new DirectoryControl[] { new AsqRequestControl("a") };
            var addResponse = typeof(AddResponse).Create<AddResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), addResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            log.SetupGet(l => l.TraceEnabled)
                .Returns(false);
            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN(connection);

            //act
            context.AddAndGet(directoryAttributes, controls);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Exactly(2));
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void AddAndGet_ValidRequestWithLogTraceEnabled_SendsRequestAndLogsIt()
        {
            //prepare
            var directoryAttributes = new DirectoryAttributes("dn", new List<KeyValuePair<string, object>>
                                                                        {
                                                                            new KeyValuePair<string, object>("att1", "pm1"),
                                                                            new KeyValuePair<string, object>("att2", null),
                                                                            new KeyValuePair<string, object>("objectClass", new[] {"oc"})
                                                                        });
            var controls = new DirectoryControl[] { new AsqRequestControl("a") };
            var addResponse = typeof(AddResponse).Create<AddResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), addResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            log.SetupGet(l => l.TraceEnabled)
                .Returns(true);
            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN(connection);

            //act
            context.AddAndGet(directoryAttributes, controls);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Exactly(2));
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Exactly(2));
        }

        [TestMethod]
        public void AddGeneric_Disposed_ThrowsObjectDisposedException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var context = new DirectoryContext(connection);

            //act
            context.Dispose();

            //assert
            Executing.This(() => context.Add(this)).Should().Throw<ObjectDisposedException>();
        }

        [TestMethod]
        public void AddGeneric_NullEntry_ThrowsArgumentNullException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var context = new DirectoryContext(connection);

            //assert
            Executing.This(() => context.Add(default(object))).Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void AddGeneric_NullEntryWithLog_ThrowsArgumentNullExceptionAndLogsIt()
        {
            //prepare
            var connection = new MockLdapConnection();
            var context = new DirectoryContext(connection);
            var log = new Mock<ILinqToLdapLogger>();
            context.Logger = log.Object;

            //assert
            Executing.This(() => context.Add(default(object))).Should().Throw<ArgumentNullException>();
            log.Verify(l => l.Error(It.IsAny<ArgumentNullException>(), "An error occurred while trying to add ''."));
        }

        [TestMethod]
        public void AddGeneric_NoObjectMapping_ThrowsMappingException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var mapper = new Mock<IDirectoryMapper>();

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            mapper.Setup(m => m.GetMapping<string>())
                .Returns(default(IObjectMapping));

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.Add("test"))
                .Should().Throw<MappingException>()
                .And.Exception.Message.Should().Be.EqualTo("Cannot add an unmapped class.");
        }

        [TestMethod]
        public void AddGeneric_NoMappedObjectClasses_ThrowsMappingException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();

            objectMapping.SetupGet(om => om.ObjectClasses)
                .Returns(new string[0]);

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            mapper.Setup(m => m.GetMapping(typeof(string)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.Add("test"))
                .Should().Throw<MappingException>()
                .And.Exception.Message.Should().Be
                .EqualTo(string.Format("Cannot add an entry without mapping objectClass for {0}.", typeof(string).FullName));
        }

        [TestMethod]
        public void AddGeneric_WithPreAddEventListener_CallsListenersEvent()
        {
            //prepare
            var obj = new object();
            var xml = new XmlDocument();
            object[] parameters;
#if (!NET35 && !NET40 && !NET45)
            parameters = new object[] { "", new System.DirectoryServices.Protocols.DirectoryControl[0], System.DirectoryServices.Protocols.ResultCode.Success, "", new System.Uri[0] };
#else
            parameters = new object[] { new XmlDocument() };
#endif
            var addResponse = typeof(AddResponse).Create<AddResponse>(parameters);
#if (NET35 || NET40 || NET45)
            addResponse.SetFieldValue("result", ResultCode.Success);
#endif
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), addResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();
            var preAdd = new Mock<IPreAddEventListener>();
            var postAdd = new Mock<IPostAddEventListener>();

            objectMapping.SetupGet(om => om.ObjectClasses)
                .Returns(new[] { "oc" });

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            configuration.Setup(c => c.GetListeners<IPreAddEventListener>())
                .Returns(new[] { preAdd.Object });

            configuration.Setup(c => c.GetListeners<IPostAddEventListener>())
                .Returns(new[] { postAdd.Object });

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.Add(obj, "dn");

            //assert
            preAdd.Verify(l => l.Notify(It.IsAny<ListenerPreArgs<object, AddRequest>>()), Times.Once());
            postAdd.Verify(l => l.Notify(It.IsAny<ListenerPostArgs<object, AddRequest, AddResponse>>()), Times.Once());
        }

        [TestMethod]
        public void AddGeneric_ValidRequestFailedResponse_ThrowsLdapException()
        {
            //prepare
            var obj = new object();
            var addResponse = typeof(AddResponse).Create<AddResponse>("dn", new DirectoryControl[0], ResultCode.StrongAuthRequired,
                                                                       "word!", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), addResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();
            var propertyMapping1 = new Mock<IPropertyMapping>();
            propertyMapping1.Setup(pm => pm.GetDirectoryAttribute(obj))
                .Returns("pm1".ToDirectoryAttribute("att1"));
            var propertyMapping2 = new Mock<IPropertyMapping>();
            propertyMapping2.Setup(pm => pm.GetDirectoryAttribute(obj))
                .Returns(new DirectoryAttributeModification { Name = "att2" });

            objectMapping.Setup(om => om.GetUpdateablePropertyMappings())
                .Returns(new List<IPropertyMapping> { propertyMapping2.Object, propertyMapping1.Object });

            objectMapping.SetupGet(om => om.ObjectClasses)
                .Returns(new[] { "oc" });

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN<object>(connection, objectMapping, mapper);

            //act
            Executing.This(() => context.Add(obj, "dn"))
                .Should().Throw<LdapException>().And.Exception.Message
                .Should().Be.EqualTo(addResponse.ToLogString());
        }

        [TestMethod]
        public void AddGeneric_ValidRequest_SendsRequest()
        {
            //prepare
            var controls = new DirectoryControl[] { new AsqRequestControl("a") };
            var obj = new object();
            var addResponse = typeof(AddResponse).Create<AddResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), addResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();
            var propertyMapping1 = new Mock<IPropertyMapping>();
            propertyMapping1.Setup(pm => pm.GetDirectoryAttribute(obj))
                .Returns("pm1".ToDirectoryAttribute("att1"));
            var propertyMapping2 = new Mock<IPropertyMapping>();
            propertyMapping2.Setup(pm => pm.GetDirectoryAttribute(obj))
                .Returns(new DirectoryAttributeModification { Name = "att2" });

            objectMapping.Setup(om => om.GetUpdateablePropertyMappings())
                .Returns(new List<IPropertyMapping> { propertyMapping2.Object, propertyMapping1.Object });

            objectMapping.SetupGet(om => om.ObjectClasses)
                .Returns(new[] { "oc" });

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN<object>(connection, objectMapping, mapper);

            //act
            context.Add(obj, "dn", controls);

            //assert
            var addRequest = connection.SentRequests[0].CastTo<AddRequest>();
            addRequest.Attributes.Count.Should().Be.EqualTo(2);
            addRequest.Attributes[0].Name.Should().Be.EqualTo("att1");
            addRequest.Attributes[0][0].Should().Be.EqualTo("pm1");
            addRequest.Attributes[1].Name.Should().Be.EqualTo("objectClass");
            addRequest.Attributes[1][0].Should().Be.EqualTo("oc");
            addRequest.DistinguishedName.Should().Be.EqualTo("dn");
            addRequest.Controls.Cast<DirectoryControl>().Should().Have.SameSequenceAs(controls);
        }

        [TestMethod]
        public void AddAndGetGeneric_ValidRequestWithLogTraceDisabled_SendsRequestAndDoesNotLogIt()
        {
            //prepare
            var controls = new DirectoryControl[] { new AsqRequestControl("a") };
            var obj = new object();
            var addResponse = typeof(AddResponse).Create<AddResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), addResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();
            var propertyMapping1 = new Mock<IPropertyMapping>();
            propertyMapping1.Setup(pm => pm.GetDirectoryAttribute(obj))
                .Returns("pm1".ToDirectoryAttribute("att1"));
            var propertyMapping2 = new Mock<IPropertyMapping>();
            propertyMapping2.Setup(pm => pm.GetDirectoryAttribute(obj))
                .Returns(new DirectoryAttributeModification { Name = "att2" });

            objectMapping.Setup(om => om.GetUpdateablePropertyMappings())
                .Returns(new List<IPropertyMapping> { propertyMapping2.Object, propertyMapping1.Object });

            objectMapping.SetupGet(om => om.ObjectClasses)
                .Returns(new[] { "oc" });

            log.SetupGet(l => l.TraceEnabled)
                .Returns(false);

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN<object>(connection, objectMapping, mapper);

            //act
            context.AddAndGet(obj, "dn", controls);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Exactly(2));
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void AddAndGetGeneric_ValidRequestWithLogTraceEnabled_SendsRequestAndLogIt()
        {
            //prepare
            var controls = new DirectoryControl[] { new AsqRequestControl("a") };
            var obj = new object();
            var addResponse = typeof(AddResponse).Create<AddResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(AddRequest), addResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            var mapper = new Mock<IDirectoryMapper>();
            var objectMapping = new Mock<IObjectMapping>();
            var propertyMapping1 = new Mock<IPropertyMapping>();
            propertyMapping1.Setup(pm => pm.GetDirectoryAttribute(obj))
                .Returns("pm1".ToDirectoryAttribute("att1"));
            var propertyMapping2 = new Mock<IPropertyMapping>();
            propertyMapping2.Setup(pm => pm.GetDirectoryAttribute(obj))
                .Returns(new DirectoryAttributeModification { Name = "att2" });

            objectMapping.Setup(om => om.GetUpdateablePropertyMappings())
                .Returns(new List<IPropertyMapping> { propertyMapping2.Object, propertyMapping1.Object });

            objectMapping.SetupGet(om => om.ObjectClasses)
                .Returns(new[] { "oc" });

            log.SetupGet(l => l.TraceEnabled)
                .Returns(true);

            configuration.SetupGet(c => c.Mapper)
                .Returns(mapper.Object);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            mapper.Setup(m => m.GetMapping(typeof(object)))
                .Returns(objectMapping.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            MockGetByDN<object>(connection, objectMapping, mapper);

            //act
            context.AddAndGet(obj, "dn", controls);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Exactly(2));
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Exactly(2));
        }

        #endregion Add Tests

        #region GetDistinguishedName Tests

        [TestMethod]
        public void GetDistinguishedName_DistinguishedName_ReturnsDistinguishedName()
        {
            //act
            var dn = DirectoryContext.GetDistinguishedName<object>("test", null, null);

            //assert
            dn.Should().Be.EqualTo("test");
        }

        [TestMethod]
        public void GetDistinguishedName_MappedDistinguishedName_ReturnsDistinguishedName()
        {
            //prepare
            var objectMapping = new Mock<IObjectMapping>();
            var propertyMapping = new Mock<IPropertyMapping>();
            var obj = new object();

            objectMapping.Setup(om => om.GetDistinguishedNameMapping())
                .Returns(propertyMapping.Object);

            propertyMapping.Setup(pm => pm.GetValue(obj))
                .Returns("test");

            //act
            var dn = DirectoryContext.GetDistinguishedName(null, objectMapping.Object, obj);

            //assert
            dn.Should().Be.EqualTo("test");
        }

        [TestMethod]
        public void GetDistinguishedName_ObjectMappingWithoutMappedDistinguishedName_ThrowsMappingException()
        {
            //prepare
            var objectMapping = new Mock<IObjectMapping>();

            objectMapping.Setup(om => om.GetDistinguishedNameMapping())
                .Returns(default(IPropertyMapping));

            //assert
            Executing.This(() => DirectoryContext.GetDistinguishedName<object>(null, objectMapping.Object, null))
                .Should().Throw<MappingException>().And.Exception.Message
                .Should().Be.EqualTo("Distinguished name must be mapped.");
        }

        [TestMethod]
        public void GetDistinguishedName_MappedDistinguishedNameReturnsNull_ThrowsMappingException()
        {
            //prepare
            var objectMapping = new Mock<IObjectMapping>();
            var propertyMapping = new Mock<IPropertyMapping>();
            var obj = new object();

            objectMapping.Setup(om => om.GetDistinguishedNameMapping())
                .Returns(propertyMapping.Object);

            // ReSharper disable ImplicitlyCapturedClosure
            propertyMapping.Setup(pm => pm.GetValue(obj))
                // ReSharper restore ImplicitlyCapturedClosure
                .Returns(default(string));

            //assert
            Executing.This(() => DirectoryContext.GetDistinguishedName(null, objectMapping.Object, obj))
                .Should().Throw<ArgumentException>().And.Exception.Message
                .Should().Be.EqualTo("The distinguished name cannot be null or empty.");
        }

        #endregion GetDistinguishedName Tests

        #region Delete Tests

        [TestMethod]
        public void Delete_EmptyDistinguishedName_ThrowsArgumentNullException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.Delete(""))
                .Should().Throw<ArgumentNullException>().And.Exception.ParamName
                .Should().Be.EqualTo("distinguishedName");
        }

        [TestMethod]
        public void Delete_CausesExceptionWithLog_ThrowsAndLogsException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.Delete(""))
                .Should().Throw<ArgumentNullException>().And.Exception.ParamName
                .Should().Be.EqualTo("distinguishedName");

            log.Verify(l => l.Error(It.IsAny<ArgumentNullException>(), "An error occurred while trying to delete ''."));
        }

        [TestMethod]
        public void Delete_WithEventListener_Notifies()
        {
            //prepare
            var xml = new XmlDocument();
            object[] parameters;
#if (!NET35 && !NET40 && !NET45)
            parameters = new object[] { "", new System.DirectoryServices.Protocols.DirectoryControl[0], System.DirectoryServices.Protocols.ResultCode.Success, "", new System.Uri[0] };
#else
            parameters = new object[] { new XmlDocument() };
#endif
            var deleteResponse = typeof(DeleteResponse).Create<DeleteResponse>(parameters);
#if (NET35 || NET40 || NET45)
            deleteResponse.SetFieldValue("result", ResultCode.Success);
#endif
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(DeleteRequest), deleteResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var preAdd = new Mock<IPreDeleteEventListener>();
            var postAdd = new Mock<IPostDeleteEventListener>();

            configuration.Setup(c => c.GetListeners<IDeleteEventListener>())
                .Returns(new IDeleteEventListener[] { preAdd.Object, postAdd.Object });

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.Delete("dn");

            //assert
            preAdd.Verify(l => l.Notify(It.IsAny<ListenerPreArgs<string, DeleteRequest>>()), Times.Once());
            postAdd.Verify(l => l.Notify(It.IsAny<ListenerPostArgs<string, DeleteRequest, DeleteResponse>>()), Times.Once());
        }

        [TestMethod]
        public void Delete_ValidRequestWithoutLog_SendsRequest()
        {
            //prepare
            var controls = new DirectoryControl[] { new TreeDeleteControl() };
            var deleteResponse = typeof(DeleteResponse).Create<DeleteResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(DeleteRequest), deleteResponse } });
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.Delete("dn", controls);

            //assert
            var request = connection.SentRequests[0].CastTo<DeleteRequest>();
            request.Controls.Cast<DirectoryControl>().Should().Have.SameSequenceAs(controls);
            request.DistinguishedName.Should().Be.EqualTo("dn");
        }

        [TestMethod]
        public void Delete_ValidRequestFailedResponse_ThrowsLdapException()
        {
            //prepare
            var deleteResponse = typeof(DeleteResponse).Create<DeleteResponse>("dn", new DirectoryControl[0], ResultCode.UnwillingToPerform,
                                                                       "huh?", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(DeleteRequest), deleteResponse } });
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.Delete("dn"))
                .Should().Throw<LdapException>().And.Exception.Message
                .Should().Be.EqualTo(deleteResponse.ToLogString());
        }

        [TestMethod]
        public void Delete_ValidRequestWithLogTraceDisabled_SendsRequestAndDoesNotLogIt()
        {
            //prepare
            var controls = new DirectoryControl[] { new TreeDeleteControl() };
            var deleteResponse = typeof(DeleteResponse).Create<DeleteResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(DeleteRequest), deleteResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            log.SetupGet(l => l.TraceEnabled)
                .Returns(false);
            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.Delete("dn", controls);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Once());
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void Delete_ValidRequestWithLogTraceEnabled_SendsRequestAndLogsIt()
        {
            //prepare
            var controls = new DirectoryControl[] { new TreeDeleteControl() };
            var deleteResponse = typeof(DeleteResponse).Create<DeleteResponse>("dn", new DirectoryControl[0], ResultCode.Success,
                                                                       "", new Uri[0]);
            var connection = new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(DeleteRequest), deleteResponse } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            log.SetupGet(l => l.TraceEnabled)
                .Returns(true);
            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.Delete("dn", controls);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Once());
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Once());
        }

        #endregion Delete Tests

        #region MoveEntry Tests

        [TestMethod]
        public void MoveEntry_NullCurrentDistinguishedName_ThrowsArgumentNullException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.MoveEntry(null, "new"))
                .Should().Throw<ArgumentNullException>().And.Exception.ParamName
                .Should().Be.EqualTo("currentDistinguishedName");
        }

        [TestMethod]
        public void MoveEntry_NullCurrentDistinguishedNameWithLog_ThrowsArgumentExceptionAndLogsIt()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.MoveEntry(null, "new"))
                .Should().Throw<ArgumentNullException>();

            log.Verify(l => l.Error(It.IsAny<ArgumentNullException>(), "An error occurred while trying to move entry '' to 'new'."));
        }

        [TestMethod]
        public void MoveEntry_NullNewNamingContext_ThrowsArgumentNullException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.MoveEntry("dn", null))
                .Should().Throw<ArgumentNullException>().And.Exception.ParamName
                .Should().Be.EqualTo("newNamingContext");
        }

        [TestMethod]
        public void MoveEntry_ValidRequest_SendsRequest()
        {
            //prepare
            var controls = new DirectoryControl[] { new AsqRequestControl() };
            const string dn = "CN=Test,OU=container,DC=server,DC=com";
            const string newContainer = "OU=new container,DC=server,DC=com";
            var response = typeof(ModifyDNResponse)
                .Create<ModifyDNResponse>("new dn", new DirectoryControl[0], ResultCode.Success, "", new Uri[0]);
            var connection =
                new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyDNRequest), response } });
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            var newDn = context.MoveEntry(dn, newContainer, null, controls);

            //assert
            newDn.Should().Be.EqualTo("CN=Test,OU=new container,DC=server,DC=com");
            var request = connection.SentRequests[0].CastTo<ModifyDNRequest>();
            request.Controls.OfType<DirectoryControl>().Should().Have.SameSequenceAs(controls);
            request.DistinguishedName.Should().Be.EqualTo(dn);
            request.NewParentDistinguishedName.Should().Be.EqualTo(newContainer);
            request.NewName.Should().Be.EqualTo("CN=Test");
        }

        [TestMethod]
        public void MoveEntry_ValidRequestWithLogTraceDisabled_SendsRequestAndDoesNotLogIt()
        {
            //prepare
            var controls = new DirectoryControl[] { new AsqRequestControl() };
            const string dn = "CN=Test,OU=container,DC=server,DC=com";
            const string newContainer = "OU=new container,DC=server,DC=com";
            var response = typeof(ModifyDNResponse)
                .Create<ModifyDNResponse>("new dn", new DirectoryControl[0], ResultCode.Success, "", new Uri[0]);
            var connection =
                new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyDNRequest), response } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            log.SetupGet(l => l.TraceEnabled)
                .Returns(false);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.MoveEntry(dn, newContainer, null, controls);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Once());
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void MoveEntry_ValidRequestWithLogTraceEnabled_SendsRequestAndLogsIt()
        {
            //prepare
            var controls = new DirectoryControl[] { new AsqRequestControl() };
            const string dn = "CN=Test,OU=container,DC=server,DC=com";
            const string newContainer = "OU=new container,DC=server,DC=com";
            var response = typeof(ModifyDNResponse)
                .Create<ModifyDNResponse>("new dn", new DirectoryControl[0], ResultCode.Success, "", new Uri[0]);
            var connection =
                new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyDNRequest), response } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            log.SetupGet(l => l.TraceEnabled)
                .Returns(true);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.MoveEntry(dn, newContainer, null, controls);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Once());
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void MoveEntry_ValidRequestWithFailedResponse_ThrowsLdapException()
        {
            //prepare
            const string dn = "CN=Test,OU=container,DC=server,DC=com";
            const string newContainer = "OU=new container,DC=server,DC=com";
            var response = typeof(ModifyDNResponse)
                .Create<ModifyDNResponse>("new dn", new DirectoryControl[0], ResultCode.UnwillingToPerform, "nope!", new Uri[0]);
            var connection =
                new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyDNRequest), response } });
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.MoveEntry(dn, newContainer))
                .Should().Throw<LdapException>().And.Exception.Message
                .Should().Be.EqualTo(response.ToLogString());
        }

        #endregion MoveEntry Tests

        #region RenameEntry Tests

        [TestMethod]
        public void RenameEntry_NullCurrentDistinguishedName_ThrowsArgumentNullException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.RenameEntry(null, "new"))
                .Should().Throw<ArgumentNullException>().And.Exception.ParamName
                .Should().Be.EqualTo("currentDistinguishedName");
        }

        [TestMethod]
        public void RenameEntry_NullCurrentDistinguishedNameWithLog_ThrowsArgumentExceptionAndLogsIt()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.RenameEntry(null, "new"))
                .Should().Throw<ArgumentNullException>();

            log.Verify(l => l.Error(It.IsAny<ArgumentNullException>(), "An error occurred while trying to rename entry '' to 'new'."));
        }

        [TestMethod]
        public void RenameEntry_NullNewName_ThrowsArgumentNullException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.RenameEntry("dn", null))
                .Should().Throw<ArgumentNullException>().And.Exception.ParamName
                .Should().Be.EqualTo("newName");
        }

        [TestMethod]
        public void RenameEntry_ValidRequest_SendsRequest()
        {
            //prepare
            var controls = new DirectoryControl[] { new AsqRequestControl() };
            const string dn = "CN=Test,OU=container,DC=server,DC=com";
            const string newName = "Another Test";
            var response = typeof(ModifyDNResponse)
                .Create<ModifyDNResponse>("new dn", new DirectoryControl[0], ResultCode.Success, "", new Uri[0]);
            var connection =
                new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyDNRequest), response } });
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            var newDn = context.RenameEntry(dn, newName, null, controls);

            //assert
            newDn.Should().Be.EqualTo("CN=Another Test,OU=container,DC=server,DC=com");
            var request = connection.SentRequests[0].CastTo<ModifyDNRequest>();
            request.Controls.OfType<DirectoryControl>().Should().Have.SameSequenceAs(controls);
            request.DistinguishedName.Should().Be.EqualTo(dn);
            request.NewParentDistinguishedName.Should().Be.EqualTo("OU=container,DC=server,DC=com");
            request.NewName.Should().Be.EqualTo("CN=Another Test");
        }

        [TestMethod]
        public void RenameEntry_ValidRequestWithLogTraceDisabled_SendsRequestAndDoesNotLogIt()
        {
            //prepare
            var controls = new DirectoryControl[] { new AsqRequestControl() };
            const string dn = "CN=Test,OU=container,DC=server,DC=com";
            const string newName = "Another Test";
            var response = typeof(ModifyDNResponse)
                .Create<ModifyDNResponse>("new dn", new DirectoryControl[0], ResultCode.Success, "", new Uri[0]);
            var connection =
                new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyDNRequest), response } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            log.SetupGet(l => l.TraceEnabled)
                .Returns(false);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.RenameEntry(dn, newName, null, controls);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Once());
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Never());
        }

        [TestMethod]
        public void RenameEntry_ValidRequestWithLogTraceEnabled_SendsRequestAndLogsIt()
        {
            //prepare
            var controls = new DirectoryControl[] { new AsqRequestControl() };
            const string dn = "CN=Test,OU=container,DC=server,DC=com";
            const string newName = "Another Test";
            var response = typeof(ModifyDNResponse)
                .Create<ModifyDNResponse>("new dn", new DirectoryControl[0], ResultCode.Success, "", new Uri[0]);
            var connection =
                new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyDNRequest), response } });
            var configuration = new Mock<ILdapConfiguration>();
            var log = new Mock<ILinqToLdapLogger>();
            log.SetupGet(l => l.TraceEnabled)
                .Returns(true);

            configuration.SetupGet(c => c.Log)
                .Returns(log.Object);

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //act
            context.RenameEntry(dn, newName, null, controls);

            //assert
            log.Verify(l => l.TraceEnabled, Times.Once());
            log.Verify(l => l.Trace(It.IsAny<string>()), Times.Once());
        }

        [TestMethod]
        public void RenameEntry_ValidRequestWithFailedResponse_ThrowsLdapException()
        {
            //prepare
            const string dn = "CN=Test,OU=container,DC=server,DC=com";
            const string newName = "OU=new container,DC=server,DC=com";
            var response = typeof(ModifyDNResponse)
                .Create<ModifyDNResponse>("new dn", new DirectoryControl[0], ResultCode.UnwillingToPerform, "nope!", new Uri[0]);
            var connection =
                new MockLdapConnection(new Dictionary<Type, DirectoryResponse> { { typeof(ModifyDNRequest), response } });
            var configuration = new Mock<ILdapConfiguration>();

            var context = new DirectoryContext(connection, configuration: configuration.Object);

            //assert
            Executing.This(() => context.RenameEntry(dn, newName))
                .Should().Throw<LdapException>().And.Exception.Message
                .Should().Be.EqualTo(response.ToLogString());
        }

        #endregion RenameEntry Tests

        #region Dispose Tests

        [TestMethod]
        public void Dispose_CalledMultipleTimes_DoesNotCauseException()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();

            //assert
            Executing.This(() =>
                               {
                                   var context = new DirectoryContext(connection, true, configuration.Object);
                                   context.Dispose();
                                   context.Dispose();
                               })
                .Should().NotThrow();
            connection.TimesDisposed.Should().Be.EqualTo(1);
        }

        [TestMethod]
        public void Dispose_DisposeOfConnectionTrue_DisposesConnectionAndNullsConfiguration()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var context = new DirectoryContext(connection, true, configuration.Object);

            //act
            context.Dispose();

            //assert
            connection.TimesDisposed.Should().Be.EqualTo(1);
            context.FieldValueEx<LdapConnection>("_connection").Should().Be.Null();
            context.FieldValueEx<ILdapConfiguration>("_configuration").Should().Be.Null();
        }

        [TestMethod]
        public void Dispose_DisposeOfConnectionFalse_NullsConnectionAndNullsConfiguration()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var context = new DirectoryContext(connection, false, configuration.Object);

            //act
            context.Dispose();

            //assert
            connection.TimesDisposed.Should().Be.EqualTo(0);
            context.FieldValueEx<LdapConnection>("_connection").Should().Be.Null();
            context.FieldValueEx<ILdapConfiguration>("_configuration").Should().Be.Null();
        }

        [TestMethod]
        public void Dispose_IgnoresFinalizer()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();

            //act
            using (new DirectoryContext(connection, true, configuration.Object)) { }
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //assert
            connection.TimesDisposed.Should().Be.EqualTo(1);
        }

        [TestMethod]
        public void Dispose_With_ConnectionFactory_Calls_Release_()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var connectionFactory = new Mock<ILdapConnectionFactory>();
            configuration.Setup(x => x.ConnectionFactory)
                .Returns(connectionFactory.Object);
            connectionFactory.Setup(x => x.GetConnection())
                .Returns(connection);

            //act
            using (new DirectoryContext(configuration.Object)) { }
            GC.Collect();
            GC.WaitForPendingFinalizers();

            //assert
            connectionFactory.Verify(x => x.ReleaseConnection(connection), Times.Once());
        }

        [TestMethod]
        public void Finalizer_GetsCalled_But_Does_Not_Dispose()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();

            //act
            // ReSharper disable ObjectCreationAsStatement
            new DirectoryContext(connection, true, configuration.Object);
            // ReSharper restore ObjectCreationAsStatement

            GC.Collect();
            GC.WaitForPendingFinalizers();

            //assert
            connection.TimesDisposed.Should().Be.EqualTo(0);
        }

#if (NET35 || NET40 || NET45)
        [TestMethod]
        public void Finalizer_Returns_Connection_To_Pool()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var connectionFactory = new Mock<IPooledLdapConnectionFactory>();
            configuration.Setup(x => x.ConnectionFactory)
                .Returns(connectionFactory.Object);
            connectionFactory.Setup(x => x.GetConnection())
                .Returns(connection);

            //act
            new DirectoryContext(configuration.Object);

            GC.Collect();
            GC.WaitForPendingFinalizers();

            //assert
            connectionFactory.Verify(x => x.ReleaseConnection(connection), Times.Once());
        }
#endif

        [TestMethod]
        public void Finalizer_Does_Not_Release_Connection_For_Non_Pooled_Factory()
        {
            //prepare
            var connection = new MockLdapConnection();
            var configuration = new Mock<ILdapConfiguration>();
            var connectionFactory = new Mock<ILdapConnectionFactory>();
            configuration.Setup(x => x.ConnectionFactory)
                .Returns(connectionFactory.Object);
            connectionFactory.Setup(x => x.GetConnection())
                .Returns(connection);

            //act
            // ReSharper disable ObjectCreationAsStatement
            new DirectoryContext(configuration.Object);
            // ReSharper restore ObjectCreationAsStatement

            GC.Collect();
            GC.WaitForPendingFinalizers();

            //assert
            connection.TimesDisposed.Should().Be.EqualTo(0);
            connectionFactory.Verify(x => x.ReleaseConnection(connection), Times.Never());
        }

        #endregion Dispose Tests

        private static void MockGetByDN<T>(MockLdapConnection connection, Mock<IObjectMapping> mapping, Mock<IDirectoryMapper> mapper)
            where T : class
        {
            var searchResponse = typeof(SearchResponse).Create<SearchResponse>("dn", new DirectoryControl[0],
                                                                                ResultCode.Success, "", new Uri[0]);

            connection.RequestResponses.Add(typeof(SearchRequest), searchResponse);

            mapper.Setup(m => m.Map<T>(null, null, null, null))
                .Returns(mapping.Object);

            mapping.Setup(m => m.Properties)
                .Returns(new KeyValuePair<string, string>[0].ToDictionary(x => x.Key, x => x.Value).ToReadOnlyDictionary());
        }

        private static void MockGetByDN(MockLdapConnection connection)
        {
            var searchResponse = typeof(SearchResponse).Create<SearchResponse>("dn", new DirectoryControl[0],
                                                                                ResultCode.Success, "", new Uri[0]);

            connection.RequestResponses.Add(typeof(SearchRequest), searchResponse);
        }
    }
}