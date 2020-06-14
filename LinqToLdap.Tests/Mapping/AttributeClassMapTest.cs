using System.Collections.Generic;
using System.Linq;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

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
            mapping.FieldValueEx<string>("_namingContext").Should().Be.EqualTo("name");
            mapping.FieldValueEx<string>("_objectCategory").Should().Be.EqualTo("oc");
            mapping.FieldValueEx<IEnumerable<string>>("_objectClass").Should().Have.SameSequenceAs(new[] { "cl" });
            var properties = mapping.PropertyMappings.ToList();

            properties.Should().Have.Count.EqualTo(4);

            properties[0].As<PropertyMappingBuilder<AttributeClassMapTest, int?>>().AttributeName.Should().Be.EqualTo("prop1");
            properties[1].As<PropertyMappingBuilder<AttributeClassMapTest, byte[]>>().AttributeName.Should().Be.Null();
            properties[1].As<PropertyMappingBuilder<AttributeClassMapTest, byte[]>>().PropertyInfo.Name.Should().Be.EqualTo("Property3");
            properties[2].As<PropertyMappingBuilder<AttributeClassMapTest, string>>().AttributeName.Should().Be.EqualTo("ou");
            properties[3].As<PropertyMappingBuilder<AttributeClassMapTest, string>>().AttributeName.Should().Be.EqualTo("distinguishedname");
        }

        [TestMethod]
        public void New_SubClassMappedClass_MapsCorrectly()
        {
            //act
            var mapping = new AttributeClassMap<AttributeClassMapSubTest>().PerformMapping();

            //assert
            mapping.FieldValueEx<string>("_namingContext").Should().Be.EqualTo("name");
            mapping.FieldValueEx<string>("_objectCategory").Should().Be.EqualTo("oc");
            mapping.FieldValueEx<IEnumerable<string>>("_objectClass").Should().Have.SameSequenceAs(new[] { "cl" });
            var properties = mapping.FieldValueEx<List<IPropertyMappingBuilder>>("PropertyMappings").ToList();

            properties.Should().Have.Count.EqualTo(4);

            properties[0].As<PropertyMappingBuilder<AttributeClassMapSubTest, string[]>>().AttributeName.Should().Be.Null();
            properties[0].As<PropertyMappingBuilder<AttributeClassMapSubTest, string[]>>().PropertyInfo.Name.Should().Be.EqualTo("Property4");
            properties[1].As<PropertyMappingBuilder<AttributeClassMapSubTest, int?>>().AttributeName.Should().Be.EqualTo("prop1");
            properties[2].As<PropertyMappingBuilder<AttributeClassMapSubTest, string>>().AttributeName.Should().Be.EqualTo("ou");
            properties[3].As<PropertyMappingBuilder<AttributeClassMapSubTest, string>>().AttributeName.Should().Be.EqualTo("distinguishedname");
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