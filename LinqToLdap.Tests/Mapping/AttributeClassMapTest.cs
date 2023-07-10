using System.Collections.Generic;
using System.Linq;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;

namespace LinqToLdap.Tests.Mapping
{
    [TestClass]
    [DirectorySchema("name", ObjectCategory = "oc", ObjectClasses = new[] { "cl" })]
    public class AttributeClassMapTest
    {
        [DirectoryAttribute("prop1")]
        public int? Property1 { get; protected set; }

        [DirectoryAttribute]
        public string Property2 { set { } }

        [DirectoryAttribute]
        public virtual byte[] Property3 { get; set; }

        [DirectoryAttribute]
        public string[] Property4 { get { return null; } }

        [DistinguishedName]
        public string Property5 { get; set; }

        [DirectoryAttribute("ou", true)]
        protected string Property6 { get; set; }

        [TestMethod]
        public void New_MappedClass_MapsCorrectly()
        {
            //act
            var mapping = new AttributeClassMap<AttributeClassMapTest>().PerformMapping().CastTo<AttributeClassMap<AttributeClassMapTest>>();

            //assert
            mapping.FieldValueEx<string>("_namingContext").Should().Be("name");
            mapping.FieldValueEx<string>("_objectCategory").Should().Be("oc");
            mapping.FieldValueEx<IEnumerable<string>>("_objectClass").Should().ContainInOrder(new[] { "cl" });
            var properties = mapping.PropertyMappings.ToList();

            properties.Should().HaveCount(4);

            properties[0].CastTo<PropertyMappingBuilder<AttributeClassMapTest, int?>>().AttributeName.Should().Be("prop1");
            properties[1].CastTo<PropertyMappingBuilder<AttributeClassMapTest, byte[]>>().AttributeName.Should().BeNull();
            properties[1].CastTo<PropertyMappingBuilder<AttributeClassMapTest, byte[]>>().PropertyInfo.Name.Should().Be("Property3");
            properties[2].CastTo<PropertyMappingBuilder<AttributeClassMapTest, string>>().AttributeName.Should().Be("ou");
            properties[3].CastTo<PropertyMappingBuilder<AttributeClassMapTest, string>>().AttributeName.Should().Be("distinguishedname");
        }

        [TestMethod]
        public void New_SubClassMappedClass_MapsCorrectly()
        {
            //act
            var mapping = new AttributeClassMap<AttributeClassMapSubTest>().PerformMapping();

            //assert
            mapping.FieldValueEx<string>("_namingContext").Should().Be("name");
            mapping.FieldValueEx<string>("_objectCategory").Should().Be("oc");
            mapping.FieldValueEx<IEnumerable<string>>("_objectClass").Should().ContainInOrder(new[] { "cl" });
            var properties = mapping.FieldValueEx<List<IPropertyMappingBuilder>>("PropertyMappings").ToList();

            properties.Should().HaveCount(4);

            properties[0].CastTo<PropertyMappingBuilder<AttributeClassMapSubTest, string[]>>().AttributeName.Should().BeNull();
            properties[0].CastTo<PropertyMappingBuilder<AttributeClassMapSubTest, string[]>>().PropertyInfo.Name.Should().Be("Property4");
            properties[1].CastTo<PropertyMappingBuilder<AttributeClassMapSubTest, int?>>().AttributeName.Should().Be("prop1");
            properties[2].CastTo<PropertyMappingBuilder<AttributeClassMapSubTest, string>>().AttributeName.Should().Be("ou");
            properties[3].CastTo<PropertyMappingBuilder<AttributeClassMapSubTest, string>>().AttributeName.Should().Be("distinguishedname");
        }
    }

    public class AttributeClassMapSubTest : AttributeClassMapTest
    {
        [DirectoryAttribute]
        protected string[] Property4 { get; private set; }

        public override byte[] Property3
        {
            get
            {
                return base.Property3;
            }
            set
            {
                base.Property3 = value;
            }
        }
    }
}