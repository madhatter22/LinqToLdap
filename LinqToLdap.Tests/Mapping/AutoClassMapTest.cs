using System;
using System.Collections.Generic;
using System.Linq;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Mapping
{
    [TestClass]
    public class AutoClassMapTest
    {
        public string EntryDn { get; set; }
        public string Ou { get; set; }
        public int? Property1 { get; set; }
        public string Property2 { set { } }
        public byte[] Property3 { get; set; }
        public string[] Property4 { get { return null; } }

        [TestMethod]
        public void AutoClassMap_AnonymouseType_MapsAllProperties()
        {
            var anon =
                new
                {
                    DistinguishedName = default(string),
                    Cn = default(string),
                    Property1 = default(int?),
                    Property2 = default(string),
                    Property3 = DateTime.Now,
                    Property4 = new Guid(),
                    Property5 = default(byte[]),
                    Property6 = default(string[])
                };
            AssertClassMap(anon, "container", "oc", new List<string> { "cl" });
        }

        [TestMethod]
        public void AutoClassMap_StandardType_MapsOnlyPropertiesWithGettersAndSetters()
        {
            var mapping = new AutoClassMap<AutoClassMapTest>().PerformMapping("container", "oc", false, new List<string> { "cl" }, false)
                .CastTo<AutoClassMap<AutoClassMapTest>>();
            var propertyMappings = mapping.PropertyMappings;
            propertyMappings[0].IsDistinguishedName.Should().BeTrue();
            propertyMappings[1].ReadOnlyConfiguration.Should().Be(ReadOnly.Always);
            propertyMappings.Should().HaveCount(4);
            propertyMappings.Count(p => p.PropertyInfo.Name == "Property1" || p.PropertyInfo.Name == "Property3" || p.PropertyInfo.Name == "DistinguishedName" || p.PropertyInfo.Name == "Cn")
                .Should().Be(2);
            mapping.FieldValueEx<string>("_namingContext").Should().Be("container");
            mapping.FieldValueEx<string>("_objectCategory").Should().Be("oc");
            mapping.PropertyValue<bool>("IncludeObjectCategory").Should().BeFalse();
            mapping.FieldValueEx<IEnumerable<string>>("_objectClass").Should().ContainInOrder(new[] { "cl" });
            mapping.PropertyValue<bool>("IncludeObjectClasses").Should().BeFalse();
        }

        private static void AssertClassMap<T>(T example, string container, string ocategory, IEnumerable<string> oclass) where T : class
        {
            var mapping = new AutoClassMap<T>()
                .PerformMapping(container, ocategory, false, oclass, false)
                .CastTo<AutoClassMap<T>>();
            var mappedProperties = mapping.PropertyMappings;
            mappedProperties.Should().HaveCount(8);
            mappedProperties[0].IsDistinguishedName.Should().BeTrue();
            mappedProperties[1].ReadOnlyConfiguration.Should().Be(ReadOnly.Always);
            mapping.FieldValueEx<string>("_namingContext").Should().Be(container);
            mapping.PropertyValue<bool>("IncludeObjectCategory").Should().BeFalse();
            mapping.FieldValueEx<string>("_objectCategory").Should().Be("oc");
            mapping.FieldValueEx<IEnumerable<string>>("_objectClass").Should().ContainInOrder(new[] { "cl" });
            mapping.PropertyValue<bool>("IncludeObjectClasses").Should().BeFalse();
        }
    }
}