using LinqToLdap.Helpers;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using System;
using System.Linq;
using System.Reflection;

namespace LinqToLdap.Tests.Helpers
{
    public enum SomeEnum
    {
        Value1 = 0,
        Value2 = 1
    }

    [TestClass]
    public class DelegateBuilderTest
    {
        public long? Property1 { get; set; }
        public long Property2 { private get; set; }

        public DateTime? Property3 { get; set; }
        public DateTime Property4 { get; set; }

        public string Property5 { get; private set; }

        public SomeEnum Property6 { get; set; }
        protected SomeEnum? Property7 { get; private set; }

        [TestCleanup]
        public void TearDown()
        {
            Property1 = default(long?);
            Property2 = default(long);
            Property3 = default(DateTime?);
            Property4 = default(DateTime);
            Property5 = default(string);
            Property6 = default(SomeEnum);
            Property7 = default(SomeEnum?);
        }

        [TestMethod]
        public void BuidCtorWithParams_CanBuildAnonymousConstructor_BuildsConstructorThatCanParseStringsToValueTypes()
        {
            var now = DateTime.Now;
            var anon = new
            {
                Property1 = default(long?),
                Property2 = default(long),
                Property3 = default(DateTime?),
                Property4 = default(DateTime),
                Property5 = default(string),
                Property6 = default(SomeEnum),
                Property7 = default(SomeEnum?)
            };

            var ctor = GetCtorWithParams(anon);

            var instance = ctor((long?)1, (long)2, now, now, "str", SomeEnum.Value1, SomeEnum.Value2);

            instance.Property1.Should().Be.EqualTo(1);
            instance.Property2.Should().Be.EqualTo(2);
            instance.Property3.Should().Be.EqualTo(now);
            instance.Property4.Should().Be.EqualTo(now);
            instance.Property5.Should().Be.EqualTo("str");
            instance.Property6.Should().Be.EqualTo(SomeEnum.Value1);
            instance.Property7.Should().Be.EqualTo(SomeEnum.Value2);
        }

        [TestMethod]
        public void BuidUnkownCtorWithParams_CanBuildAnonymousConstructor_BuildsConstructorThatCanParseStringsToValueTypes()
        {
            var now = DateTime.Now;
            var anon = new
            {
                Property1 = default(long?),
                Property2 = default(long),
                Property3 = default(DateTime?),
                Property4 = default(DateTime),
                Property5 = default(string),
                Property6 = default(SomeEnum),
                Property7 = default(SomeEnum?)
            };

            var ctor = DelegateBuilder.BuildUnknownCtorWithParams(anon.GetType().GetConstructors().First());

            var instance = ctor((long?)1, (long)2, now, now, "str", SomeEnum.Value1, SomeEnum.Value2);

            instance.PropertyValue<long?>("Property1").Should().Be.EqualTo(1);
            instance.PropertyValue<long>("Property2").Should().Be.EqualTo(2);
            instance.PropertyValue<DateTime?>("Property3").Should().Be.EqualTo(now);
            instance.PropertyValue<DateTime>("Property4").Should().Be.EqualTo(now);
            instance.PropertyValue<string>("Property5").Should().Be.EqualTo("str");
            instance.PropertyValue<SomeEnum>("Property6").Should().Be.EqualTo(SomeEnum.Value1);
            instance.PropertyValue<SomeEnum?>("Property7").Should().Be.EqualTo(SomeEnum.Value2);
        }

        private static CtorWithParams<T> GetCtorWithParams<T>(T exmaple)
        {
            return DelegateBuilder.BuildCtorWithParams<T>(typeof(T).GetConstructors().First());
        }

        [TestMethod]
        public void BuildGetter_AnonymousType_BuildsGetters()
        {
            var anon = new
            {
                Property1 = (long?)1,
                Property2 = (long)2,
                Property3 = (DateTime?)DateTime.Now,
                Property4 = (DateTime)DateTime.Now,
                Property5 = "str",
                Property6 = SomeEnum.Value2,
                Property7 = (SomeEnum?)SomeEnum.Value2
            };

            int count = 0;
            foreach (var property in anon.GetType().GetProperties())
            {
                var getter = GetGetter(anon, property);
                var value = getter(anon);
                value.Should().Not.Be.Null();
                value.Should().Be.EqualTo(property.GetValue(anon, null));
                count++;
            }

            count.Should().Be.EqualTo(7);
        }

        [TestMethod]
        public void BuildGetter_StandardType_BuildsGetters()
        {
            Property1 = 1;
            Property2 = 2;
            Property3 = DateTime.Now;
            Property4 = DateTime.Now;
            Property5 = "str";
            Property6 = SomeEnum.Value1;
            Property7 = SomeEnum.Value2;

            int count = 0;
            foreach (var property in GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
            {
                var getter = DelegateBuilder.BuildGetter<DelegateBuilderTest>(property);
                var value = getter(this);
                value.Should().Not.Be.Null();
                value.Should().Be.EqualTo(property.GetValue(this, null));
                count++;
            }

            count.Should().Be.EqualTo(7);
        }

        [TestMethod]
        public void BuildSetter_StandardType_BuildsSetters()
        {
            var flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
            var now = DateTime.Now;
            var propertySetter1 = DelegateBuilder.BuildSetter<DelegateBuilderTest>(GetType().GetProperty("Property1", flags));
            propertySetter1(this, (long?)1);
            var propertySetter2 = DelegateBuilder.BuildSetter<DelegateBuilderTest>(GetType().GetProperty("Property2", flags));
            propertySetter2(this, (long)2);
            var propertySetter3 = DelegateBuilder.BuildSetter<DelegateBuilderTest>(GetType().GetProperty("Property3", flags));
            propertySetter3(this, now);
            var propertySetter4 = DelegateBuilder.BuildSetter<DelegateBuilderTest>(GetType().GetProperty("Property4", flags));
            propertySetter4(this, now);
            var propertySetter5 = DelegateBuilder.BuildSetter<DelegateBuilderTest>(GetType().GetProperty("Property5", flags));
            propertySetter5(this, "str");
            var propertySetter6 = DelegateBuilder.BuildSetter<DelegateBuilderTest>(GetType().GetProperty("Property6", flags));
            propertySetter6(this, SomeEnum.Value1);
            var propertySetter7 = DelegateBuilder.BuildSetter<DelegateBuilderTest>(GetType().GetProperty("Property7", flags));
            propertySetter7(this, SomeEnum.Value2);

            Property1.Should().Be.EqualTo(1);
            Property2.Should().Be.EqualTo(2);
            Property3.Should().Be.EqualTo(now);
            Property4.Should().Be.EqualTo(now);
            Property5.Should().Be.EqualTo("str");
            Property6.Should().Be.EqualTo(SomeEnum.Value1);
            Property7.Should().Be.EqualTo(SomeEnum.Value2);
        }

        [TestMethod]
        public void BuildGetter_AnonymousTypeValueTypeProperty_BuildsGetter()
        {
            var now = DateTime.Now;
            var anon = new { Property1 = now };

            var getter = GetGetter(anon, anon.GetType().GetProperty("Property1"));

            getter(anon).Should().Be.EqualTo(now);
        }

        private static Func<T, object> GetGetter<T>(T example, PropertyInfo propertyInfo)
        {
            return DelegateBuilder.BuildGetter<T>(propertyInfo);
        }
    }
}