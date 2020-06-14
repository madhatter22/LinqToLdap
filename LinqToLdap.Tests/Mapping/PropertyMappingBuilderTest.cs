using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using LinqToLdap.Collections;
using LinqToLdap.Mapping;
using LinqToLdap.Mapping.PropertyMappingBuilders;
using LinqToLdap.Mapping.PropertyMappings;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace LinqToLdap.Tests.Mapping
{
    [TestClass]
    public class PropertyMappingBuilderTest
    {
        public enum BuilderEnum
        {
            Enum1 = 0,
            Enum2 = 1
        }

        public string Property1 { get; set; }
        public DateTime Property2 { get; set; }
        public DateTime? Property3 { get; set; }
        public Guid Property4 { get; set; }
        public Guid? Property5 { get; set; }
        public byte[] Property6 { get; set; }
        public BuilderEnum Property7 { get; set; }
        public BuilderEnum? Property8 { get; set; }

        //public Bitmap Property9 { get; set; }
        public bool Property10 { get; set; }

        public bool? Property11 { get; set; }
        public int Property12 { get; set; }
        public int? Property13 { get; set; }
        public string[] Property14 { get; set; }
        public byte[][] Property15 { get; set; }
        public ICollection<string> Property16 { get; set; }
        public Collection<string> Property17 { get; set; }
        public ICollection<byte[]> Property18 { get; set; }
        public Collection<byte[]> Property19 { get; set; }
        public SecurityIdentifier Property20 { get; set; }
        public X509Certificate2 Property21 { get; set; }
        public X509Certificate Property22 { get; set; }
        public ICollection<X509Certificate2> Property23 { get; set; }
        public Collection<X509Certificate2> Property24 { get; set; }
        public ICollection<X509Certificate> Property25 { get; set; }
        public Collection<X509Certificate> Property26 { get; set; }
        public X509Certificate2[] Property27 { get; set; }
        public X509Certificate[] Property28 { get; set; }
        public DateTime?[] Property29 { get; set; }
        public DateTime[] Property30 { get; set; }
        public Collection<DateTime?> Property31 { get; set; }
        public ICollection<DateTime> Property32 { get; set; }
        public SecurityIdentifier[] Property35 { get; set; }
        public Collection<SecurityIdentifier> Property36 { get; set; }
        public ICollection<SecurityIdentifier> Property37 { get; set; }
        public IDirectoryAttributes AllProperties { get; set; }

        public string Property_33 { get; set; }
        public DateTime Property_34 { get; set; }

        [TestMethod]
        public void ToPropertyMapping_CustomPropertyWithUnderscoresAndAttributeName_ReplacesUnderscoresWithHiphens()
        {
            //prepare
            var property = GetType().GetProperty("Property_34");
            var builder = new CustomPropertyMappingBuilder<PropertyMappingBuilderTest, DateTime>(property);
            builder.As<ICustomPropertyMapper<PropertyMappingBuilderTest, DateTime>>().Named("prop34");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            mapping.AttributeName.Should().Be.EqualTo("prop34");
            mapping.PropertyName.Should().Be.EqualTo(property.Name);
        }

        [TestMethod]
        public void ToPropertyMapping_CustomPropertyWithUnderscoresAndNoAttributeName_ReplacesUnderscoresWithHiphens()
        {
            //prepare
            var property = GetType().GetProperty("Property_34");
            var builder = new CustomPropertyMappingBuilder<PropertyMappingBuilderTest, string>(property);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            mapping.AttributeName.Should().Be.EqualTo("Property-34");
            mapping.PropertyName.Should().Be.EqualTo(property.Name);
        }

        [TestMethod]
        public void ToPropertyMapping_DateTimePropertyWithUnderscoresAndAttributeName_ReplacesUnderscoresWithHiphens()
        {
            //prepare
            var property = GetType().GetProperty("Property_34");
            var builder = new DateTimePropertyMappingBuilder<PropertyMappingBuilderTest, DateTime>(property);
            builder.As<IDateTimePropertyMappingBuilder<PropertyMappingBuilderTest, DateTime>>().Named("prop34");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            mapping.AttributeName.Should().Be.EqualTo("prop34");
            mapping.PropertyName.Should().Be.EqualTo(property.Name);
        }

        [TestMethod]
        public void ToPropertyMapping_DateTimePropertyWithUnderscoresAndNoAttributeName_ReplacesUnderscoresWithHiphens()
        {
            //prepare
            var property = GetType().GetProperty("Property_34");
            var builder = new DateTimePropertyMappingBuilder<PropertyMappingBuilderTest, string>(property);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            mapping.AttributeName.Should().Be.EqualTo("Property-34");
            mapping.PropertyName.Should().Be.EqualTo(property.Name);
        }

        [TestMethod]
        public void ToPropertyMapping_StringPropertyWithUnderscoresAndAttributeName_ReplacesUnderscoresWithHiphens()
        {
            //prepare
            var property = GetType().GetProperty("Property_33");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, string>(property, false, ReadOnly.Never);
            builder.As<IPropertyMapper>().Named("prop33");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            mapping.AttributeName.Should().Be.EqualTo("prop33");
            mapping.PropertyName.Should().Be.EqualTo(property.Name);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_StringPropertyWithUnderscoresAndNoAttributeName_ReplacesUnderscoresWithHiphens()
        {
            //prepare
            var property = GetType().GetProperty("Property_33");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, string>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            mapping.AttributeName.Should().Be.EqualTo("Property-33");
            mapping.PropertyName.Should().Be.EqualTo(property.Name);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_X509Certificate2_CreatesX509Certificate2PropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property21");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, X509Certificate>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(X509Certificate2PropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property21");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_X509Certificate_CreatesX509Certificate2PropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property22");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, X509Certificate>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(X509Certificate2PropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property22");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_ByteArrayArray_CreatesByteArrayArrayPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property15");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, byte[][]>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(ByteArrayArrayPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property15");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_CollectionByteArray_CreatesByteArrayCollectionPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property19");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, Collection<byte[]>>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(ByteArrayCollectionPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property19");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_IDirectoryAttributes_CreatesCatchAllPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("AllProperties");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, IDirectoryAttributes>(property, false, ReadOnly.Always);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(CatchAllPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("AllProperties");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Always);
        }

        [TestMethod]
        public void ToPropertyMapping_ICollectionByteArray_CreatesByteArrayCollectionPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property18");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, ICollection<byte[]>>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(ByteArrayCollectionPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property18");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_CollectionString_CreatesStringCollectionPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property17");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, Collection<string>>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(StringCollectionPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property17");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_ICollectionString_CreatesStringCollectionPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property16");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, ICollection<string>>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(StringCollectionPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property16");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_CollectionX509Certificate2_CreatesX509Certificate2CollectionPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property23");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, Collection<X509Certificate2>>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(X509Certificate2CollectionPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property23");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_ICollectionX509Certificate2_CreatesX509Certificate2CollectionPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property24");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, ICollection<X509Certificate2>>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(X509Certificate2CollectionPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property24");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_CollectionX509Certificate_CreatesX509Certificate2CollectionPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property25");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, Collection<X509Certificate>>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(X509Certificate2CollectionPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property25");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_SecurityIdentifierArray_CreatesSecurityIdentifierArrayPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperties().Single(x => x.PropertyType == typeof(SecurityIdentifier[]));
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, SecurityIdentifier[]>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(SecurityIdentifierArrayPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo(property.Name);
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_SecurityIdentifierICollection_CreatesSecurityIdentifierICollectionPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperties().Single(x => x.PropertyType == typeof(ICollection<SecurityIdentifier>));
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, ICollection<SecurityIdentifier>>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(SecurityIdentifierCollectionPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo(property.Name);
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_SecurityIdentifierCollection_CreatesSecurityIdentifierCollectionPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperties().Single(x => x.PropertyType == typeof(Collection<SecurityIdentifier>));
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, Collection<SecurityIdentifier>>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(SecurityIdentifierCollectionPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo(property.Name);
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_ICollectionX509Certificate_CreatesX509Certificate2CollectionPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property26");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, ICollection<X509Certificate>>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(X509Certificate2CollectionPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property26");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_X509Certificate2Array_CreatesX509Certificate2ArrayPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property27");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, X509Certificate2[]>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(X509Certificate2ArrayPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property27");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_X509CertificateArray_CreatesX509Certificate2ArrayPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property28");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, X509Certificate[]>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(X509Certificate2ArrayPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property28");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_DateTimeAsFileTime_CreatesDateTimePropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property2");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, DateTime>(property, false, ReadOnly.Never);
            builder.As<IPropertyMapper>().DateTimeFormat(null);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(DatePropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property2");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.FieldValueEx<bool>("_isFileTimeFormat").Should().Be.True();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_NullableDateTime_CreatesDateTimePropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property3");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, DateTime?>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(DatePropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property3");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.FieldValueEx<bool>("_isFileTimeFormat").Should().Be.False();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.FieldValueEx<string>("_dateFormat").Should().Be.EqualTo("yyyyMMddHHmmss.0Z");
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_ByteArray_CreatesByteArrayPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property6");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, byte[]>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(ByteArrayPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property6");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_SecurityIdentifier_CreatesSecurityIdentifierPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property20");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, SecurityIdentifier>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(SecurityIdentifierPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property20");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_Guid_CreatesGuidPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property4");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, Guid>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(GuidPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property4");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_NullableGuid_CreatesGuidPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property5");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, Guid?>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(GuidPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property5");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_EnumStoredAsInt_CreatesEnumPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property7");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, BuilderEnum>(property, false, ReadOnly.Never);
            builder.As<IPropertyMapper>().EnumStoredAsInt();

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(EnumPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property7");
            mapping.FieldValueEx<bool>("_isStoredAsInt").Should().Be.True();
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_NullableEnum_CreatesEnumPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property8");
            var builder = new PropertyMappingBuilder<PropertyMappingBuilderTest, BuilderEnum?>(property, false, ReadOnly.Never);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(EnumPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property8");
            mapping.FieldValueEx<bool>("_isStoredAsInt").Should().Be.False();
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_StringTypeWithAttributeNameIsCommonNameAndDistinguishedName_CreatesStringPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property1");
            var builder =
                new PropertyMappingBuilder<PropertyMappingBuilderTest, string>(property, true, ReadOnly.Always);
            builder.As<IPropertyMapper>().Named("Attribute1");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(StringPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property1");
            mapping.AttributeName.Should().Be.EqualTo("Attribute1");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.True();
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Always);
            mapping.PropertyType.Should().Be.EqualTo(typeof(string));
            mapping.GetType().Should().Be.EqualTo(type);
        }

        [TestMethod]
        public void ToPropertyMapping_StringArrayTypeWithAttributeName_CreatesStringArrayPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property14");
            var builder =
                new PropertyMappingBuilder<PropertyMappingBuilderTest, string[]>(property, false, ReadOnly.Never);
            builder.As<IPropertyMapper>().Named("Attribute1");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(StringArrayPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property14");
            mapping.AttributeName.Should().Be.EqualTo("Attribute1");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.PropertyType.Should().Be.EqualTo(typeof(string[]));
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_NullableDateTimeArrayTypeWithAttributeName_CreatesDateArrayPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property29");
            var builder =
                new PropertyMappingBuilder<PropertyMappingBuilderTest, DateTime?[]>(property, false, ReadOnly.OnAdd);
            builder.As<IPropertyMapper>().Named("Attribute1");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(DateArrayPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property29");
            mapping.AttributeName.Should().Be.EqualTo("Attribute1");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.PropertyType.Should().Be.EqualTo(typeof(DateTime?[]));
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.OnAdd);
        }

        [TestMethod]
        public void ToPropertyMapping_DateTimeArrayTypeWithAttributeName_CreatesDateArrayPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property30");
            var builder =
                new PropertyMappingBuilder<PropertyMappingBuilderTest, DateTime[]>(property, false, ReadOnly.OnUpdate);
            builder.As<IPropertyMapper>().Named("Attribute1");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(DateArrayPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property30");
            mapping.AttributeName.Should().Be.EqualTo("Attribute1");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.PropertyType.Should().Be.EqualTo(typeof(DateTime[]));
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.OnUpdate);
        }

        [TestMethod]
        public void ToPropertyMapping_DateTimeCollectionTypeWithAttributeName_CreatesDateCollectionPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property31");
            var builder =
                new PropertyMappingBuilder<PropertyMappingBuilderTest, Collection<DateTime?>>(property, false, ReadOnly.Never);
            builder.As<IPropertyMapper>().Named("Attribute1");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(DateCollectionPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property31");
            mapping.AttributeName.Should().Be.EqualTo("Attribute1");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.PropertyType.Should().Be.EqualTo(typeof(Collection<DateTime?>));
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_DateTimeICollectionTypeWithAttributeName_CreatesDateCollectionPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property32");
            var builder =
                new PropertyMappingBuilder<PropertyMappingBuilderTest, Collection<DateTime?>>(property, false, ReadOnly.Never);
            builder.As<IPropertyMapper>().Named("Attribute1");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(DateCollectionPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property32");
            mapping.AttributeName.Should().Be.EqualTo("Attribute1");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.PropertyType.Should().Be.EqualTo(typeof(ICollection<DateTime>));
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_NumericTypeWithAttributeName_CreatesNumericPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property12");
            var builder =
                new PropertyMappingBuilder<PropertyMappingBuilderTest, int>(property, false, ReadOnly.Never);
            builder.As<IPropertyMapper>().Named("Attribute1");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(NumericPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property12");
            mapping.AttributeName.Should().Be.EqualTo("Attribute1");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.PropertyType.Should().Be.EqualTo(typeof(int));
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_NullableNumericTypeWithAttributeName_CreatesNumericPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property13");
            var builder =
                new PropertyMappingBuilder<PropertyMappingBuilderTest, int?>(property, false, ReadOnly.Never);
            builder.As<IPropertyMapper>().Named("Attribute1");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(NumericPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property13");
            mapping.AttributeName.Should().Be.EqualTo("Attribute1");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.PropertyType.Should().Be.EqualTo(typeof(int?));
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_BooleanTypeWithAttributeName_CreatesBooleanPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property10");
            var builder =
                new PropertyMappingBuilder<PropertyMappingBuilderTest, bool>(property, false, ReadOnly.Never);
            builder.As<IPropertyMapper>().Named("Attribute1");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(BooleanPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property10");
            mapping.AttributeName.Should().Be.EqualTo("Attribute1");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.PropertyType.Should().Be.EqualTo(typeof(bool));
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_NullableBooleanTypeWithAttributeName_CreatesBooleanPropertyMapping()
        {
            //prepare
            var property = GetType().GetProperty("Property11");
            var builder =
                new PropertyMappingBuilder<PropertyMappingBuilderTest, bool?>(property, false, ReadOnly.Never);
            builder.As<IPropertyMapper>().Named("Attribute1");

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            var type = typeof(BooleanPropertyMapping<PropertyMappingBuilderTest>);
            mapping.PropertyName.Should().Be.EqualTo("Property11");
            mapping.AttributeName.Should().Be.EqualTo("Attribute1");
            mapping.PropertyValue<Action<PropertyMappingBuilderTest, object>>("Setter").Should().Not.Be.Null();
            mapping.PropertyValue<Func<PropertyMappingBuilderTest, object>>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.PropertyType.Should().Be.EqualTo(typeof(bool?));
            mapping.GetType().Should().Be.EqualTo(type);
            mapping.ReadOnly.Should().Be.EqualTo(ReadOnly.Never);
        }

        [TestMethod]
        public void ToPropertyMapping_AnonymousType_CreatesStandardPropertyMappingWithoutSetter()
        {
            //prepare
            var anon = new { Property1 = "" };
            var property = anon.GetType().GetProperty("Property1");
            var builder = GetMappingString(anon, property);

            //act
            var mapping = builder.ToPropertyMapping();

            //assert
            mapping.PropertyValue<object>("Setter").Should().Be.Null();
            mapping.PropertyValue<object>("Getter").Should().Not.Be.Null();
            mapping.IsDistinguishedName.Should().Be.False();
            mapping.PropertyType.Should().Be.EqualTo(typeof(string));
        }

        private static PropertyMappingBuilder<T, string> GetMappingString<T>(T example, PropertyInfo property) where T : class
        {
            return new PropertyMappingBuilder<T, string>(property, false, ReadOnly.Never);
        }
    }
}