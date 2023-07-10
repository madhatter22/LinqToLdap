using System.Collections.Generic;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Mapping
{
    public class SubTestClass : TestClass
    {
        public string Property6 { get; set; }
    }

    public class SubTestClassMapping : SubClassMap<SubTestClass, TestClass>
    {
        public SubTestClassMapping() : base(new TestClassMapValid())
        {
        }

        public override IClassMap PerformMapping(string namingContext = null, string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true)
        {
            NamingContext(namingContext);
            ObjectClasses(objectClasses);
            ObjectCategory(objectCategory);

            Ignore(x => x.Property1);
            Ignore(x => x.Property5);
            Map(x => x.Property5).Named("ou").ReadOnly();
            Map(x => x.Property6);

            return this;
        }
    }

    [TestClass]
    public class SubClassMapTest
    {
        [TestMethod]
        public void Map_ValidMappingWithIgnore_HasCorrectMappedInformation()
        {
            //act
            var mapping = new SubTestClassMapping().PerformMapping("subcontainer", objectCategory: "subcategory", objectClasses: new[] { "subclass" })
                .CastTo<SubTestClassMapping>();

            //assert
            mapping.FieldValueEx<string>("_namingContext").Should().Be("subcontainer");
            mapping.FieldValueEx<string>("_objectCategory").Should().Be("subcategory");
            mapping.FieldValueEx<IEnumerable<string>>("_objectClass").Should().ContainInOrder(new[] { "subclass" });

            var propertyMappings = mapping.PropertyMappings;
            propertyMappings.Should().HaveCount(5);

            var first = propertyMappings[0].CastTo<IPropertyMappingBuilder>();
            var second = propertyMappings[3].CastTo<IPropertyMappingBuilder>();
            var third = propertyMappings[4].CastTo<IPropertyMappingBuilder>();

            first.AttributeName.Should().Be("prop2");
            first.PropertyInfo.Should().NotBeNull();

            second.ReadOnlyConfiguration.Should().Be(ReadOnly.Always);
            second.AttributeName.Should().Be("ou");
            second.PropertyInfo.Should().NotBeNull();

            third.PropertyName.Should().Be("Property6");
        }
    }
}