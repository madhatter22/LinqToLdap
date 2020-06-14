using System;
using System.Collections.Generic;
using LinqToLdap.Exceptions;
using LinqToLdap.Mapping;
using LinqToLdap.Mapping.PropertyMappingBuilders;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Mapping
{
    public class BaseTestClass
    {
        public int? Property1 { get; set; }
        public string Property3 { get; set; }
    }

    public class TestClass : BaseTestClass
    {
        public string Property2 { get; protected set; }
        public Guid Property4 { get; set; }
        public string Property5 { get; set; }
    }

    public class TestClassMapValid : ClassMap<TestClass>
    {
        public override IClassMap PerformMapping(string namingContext = null, string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true)
        {
            NamingContext("container");
            ObjectCategory("category", true);
            ObjectClasses(new List<string> { "class" }, false);

            Map(s => s.Property1)
                .EnumStoredAsInt();
            Map(s => s.Property2)
                .Named("prop2")
                .ReadOnly(ReadOnly.OnAdd);

            DistinguishedName(s => s.Property3);
            MapCustomProperty(s => s.Property4).Named("prop4")
                .ConvertFromDirectoryUsing(d =>
                {
                    var bytes = d.GetValues(typeof(byte[]))[0] as byte[];
                    return new Guid(bytes);
                })
                .ConvertToFilterUsing(v => v.ToStringOctet())
                .ConvertToDirectoryUsing(v => v.ToByteArray())
                .CompareChangesUsing((guid1, guid2) => guid1.Equals(guid2));
            Map(s => s.Property5)
                .Named("cn")
                .ReadOnly(ReadOnly.Always);

            return this;
        }
    }

    public class TestClassMapOCValid : ClassMap<TestClass>
    {
        public override IClassMap PerformMapping(string namingContext = null, string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true)
        {
            NamingContext("container");
            ObjectCategory(null, true);
            ObjectClasses(new List<string>(), true);

            Map(s => s.Property1)
                .EnumStoredAsInt();
            Map(s => s.Property2)
                .Named("prop2")
                .ReadOnly();
            DistinguishedName(s => s.Property3);
            MapCustomProperty(s => s.Property4).Named("prop4")
                .ConvertFromDirectoryUsing(d =>
                {
                    var bytes = d.GetValues(typeof(byte[]))[0] as byte[];
                    return new Guid(bytes);
                })
                .ConvertToFilterUsing(v => v.ToStringOctet())
                .ConvertToDirectoryUsing(v => v.ToByteArray())
                .CompareChangesUsing((guid1, guid2) => guid1.Equals(guid2));
            Map(s => s.Property5)
                .Named("cn")
                .ReadOnly();

            return this;
        }
    }

    public class TestClassMapTwoCommonName : ClassMap<TestClass>
    {
        public override IClassMap PerformMapping(string namingContext = null, string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true)
        {
            DistinguishedName(s => s.Property2);
            DistinguishedName(s => s.Property3);

            return this;
        }
    }

    public class TestClassMapNoProperties : ClassMap<TestClass>
    {
        public override IClassMap PerformMapping(string namingContext = null, string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true)
        {
            NamingContext("name");

            return this;
        }
    }

    [TestClass]
    public class ClassMapTest
    {
        private TestClassMapOCValid _validOCMapping;
        private TestClassMapValid _validMapping;
        private TestClassMapNoProperties _noPropertiesMapping;

        [TestInitialize]
        public void SetUp()
        {
            _validMapping = new TestClassMapValid().PerformMapping().CastTo<TestClassMapValid>();
            _noPropertiesMapping = new TestClassMapNoProperties().PerformMapping().CastTo<TestClassMapNoProperties>();
            _validOCMapping = new TestClassMapOCValid().PerformMapping().CastTo<TestClassMapOCValid>();
        }

        [TestMethod]
        public void Map_ValidMapping_HasCorrectMappedInformation()
        {
            _validMapping.FieldValueEx<string>("_namingContext").Should().Be.EqualTo("container");
            _validMapping.FieldValueEx<string>("_objectCategory").Should().Be.EqualTo("category");
            _validMapping.PropertyValue<bool>("IncludeObjectCategory").Should().Be.True();
            _validMapping.FieldValueEx<IEnumerable<string>>("_objectClass").Should().Be.Equals(new[] { "cl" });
            _validMapping.PropertyValue<bool>("IncludeObjectClasses").Should().Be.False();

            var propertyMappings = _validMapping.PropertyMappings;

            propertyMappings.Should().Have.Count.EqualTo(5);

            var first = propertyMappings[0].As<PropertyMappingBuilder<TestClass, int?>>();
            var second = propertyMappings[1].As<PropertyMappingBuilder<TestClass, string>>();
            var third = propertyMappings[2].As<PropertyMappingBuilder<TestClass, string>>();
            var fourth = propertyMappings[3].As<CustomPropertyMappingBuilder<TestClass, Guid>>();
            var fifth = propertyMappings[4].As<PropertyMappingBuilder<TestClass, string>>();

            first.AttributeName.Should().Be.Null();
            first.PropertyInfo.Should().Not.Be.Null();
            first.ReadOnlyConfiguration.Should().Be.EqualTo(ReadOnly.Never);
            first.IsEnumStoredAsInt.Should().Be.True();

            second.AttributeName.Should().Be.EqualTo("prop2");
            second.PropertyInfo.Should().Not.Be.Null();
            second.ReadOnlyConfiguration.Should().Be.EqualTo(ReadOnly.OnAdd);
            second.IsEnumStoredAsInt.Should().Be.False();

            third.IsDistinguishedName.Should().Be.True();
            third.AttributeName.Should().Be.EqualTo("distinguishedname");
            third.PropertyInfo.Should().Not.Be.Null();
            third.ReadOnlyConfiguration.Should().Be.EqualTo(ReadOnly.Always);

            fourth.ReadOnlyConfiguration.Should().Be.EqualTo(null);
            fourth.AttributeName.Should().Be.EqualTo("prop4");
            fourth.PropertyInfo.Should().Not.Be.Null();

            fifth.ReadOnlyConfiguration.Should().Be.EqualTo(ReadOnly.Always);
            fifth.AttributeName.Should().Be.EqualTo("cn");
            fifth.PropertyInfo.Should().Not.Be.Null();
        }

        [TestMethod]
        public void Map_ValidMappingWithNoOCInformation_IgnoresOC()
        {
            _validOCMapping.FieldValueEx<string>("_objectCategory").Should().Be.Null();
            _validOCMapping.PropertyValue<bool>("IncludeObjectCategory").Should().Be.False();
            _validOCMapping.FieldValueEx<IEnumerable<string>>("_objectClass").Should().Be.Null();
            _validOCMapping.PropertyValue<bool>("IncludeObjectClasses").Should().Be.False();
        }

        [TestMethod]
        public void Map_CommonNameMoreThanOnce_ThrowsMappingException()
        {
            Executing.This(() => new TestClassMapTwoCommonName().PerformMapping())
                .Should().Throw<MappingException>();
        }

        [TestMethod]
        public void Validate_NoProperties_ThrowsMappingException()
        {
            Executing.This(() => _noPropertiesMapping.Validate())
                .Should().Throw<MappingException>();
        }

        [TestMethod]
        public void Validate_ValidMapping_IsValid()
        {
            Executing.This(() => _validMapping.Validate())
                .Should().NotThrow();
        }
    }
}