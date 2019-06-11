using LinqToLdap.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace LinqToLdap.Tests.PerformanceTests
{
    public class Speed
    {
        public enum SpeedEnum
        {
            Default,
            None
        }

        public string Property1 { get; set; }
        public int Property2 { get; set; }
        public decimal Property3 { get; set; }
        public byte[] Property4 { get; set; }
        public object Property5 { get; set; }
        public int Property6 { get; set; }
        public long Property7 { get; set; }
        public double Property8 { get; set; }
        public float Property9 { get; set; }
        public SpeedEnum Property10 { get; set; }
    }

    [TestClass]
    public class DelegateSpeedTest
    {
        private const int Iterations = 5000 * 100;

        [TestMethod]
        public void New_Speed()
        {
            Speed inst = null;
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                inst = new Speed();
            }
            watch.Stop();
            Trace.WriteLine(inst.Property1);
            Trace.WriteLine(watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void DelegateBuilderNew_Speed()
        {
            Speed inst = null;
            var ctor = DelegateBuilder.BuildCtor<Speed>();
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                inst = ctor();
            }
            watch.Stop();
            Trace.WriteLine(inst.Property1);
            Trace.WriteLine(watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void ExpressionNew_Speed()
        {
            Speed inst = null;
            var ctor = (Func<Speed>)Expression.Lambda(Expression.New(typeof(Speed)))
                                                            .Compile();
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                inst = ctor();
            }
            watch.Stop();
            Trace.WriteLine(inst.Property1);
            Trace.WriteLine(watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void ActivatorNew_Speed()
        {
            Speed inst = null;
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                inst = Activator.CreateInstance(typeof(Speed)) as Speed;
            }
            watch.Stop();
            Trace.WriteLine(inst.Property1);
            Trace.WriteLine(watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Property_DirectSetter_Speed()
        {
            Speed inst = new Speed();
            long seven = 7;
            double eight = 8;
            float nine = 9;
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                inst.Property1 = "1";
                inst.Property2 = 2;
                inst.Property3 = 3m;
                inst.Property4 = new byte[0];
                inst.Property5 = "5";
                inst.Property6 = 6;
                inst.Property7 = seven;
                inst.Property8 = eight;
                inst.Property9 = nine;
                inst.Property10 = Speed.SpeedEnum.None;
            }
            watch.Stop();
            inst.Property1.Should().Be.EqualTo("1");
            inst.Property2.Should().Be.EqualTo(2);
            inst.Property3.Should().Be.EqualTo(3m);
            inst.Property4.Should().Not.Be.Null();
            inst.Property5.Should().Be.EqualTo("5");
            inst.Property6.Should().Be.EqualTo(6);
            inst.Property7.Should().Be.EqualTo(7);
            inst.Property8.Should().Be.EqualTo(8);
            inst.Property9.Should().Be.EqualTo(9);
            inst.Property10.Should().Be.EqualTo(Speed.SpeedEnum.None);
            Trace.WriteLine(watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void DelegateBuilder_PropertySetter_Speed()
        {
            Speed inst = new Speed();
            long seven = 7;
            double eight = 8;
            float nine = 9;
            var prop1Setter = DelegateBuilder.BuildSetter<Speed>(typeof(Speed).GetProperty("Property1"));
            var prop2Setter = DelegateBuilder.BuildSetter<Speed>(typeof(Speed).GetProperty("Property2"));
            var prop3Setter = DelegateBuilder.BuildSetter<Speed>(typeof(Speed).GetProperty("Property3"));
            var prop4Setter = DelegateBuilder.BuildSetter<Speed>(typeof(Speed).GetProperty("Property4"));
            var prop5Setter = DelegateBuilder.BuildSetter<Speed>(typeof(Speed).GetProperty("Property5"));
            var prop6Setter = DelegateBuilder.BuildSetter<Speed>(typeof(Speed).GetProperty("Property6"));
            var prop7Setter = DelegateBuilder.BuildSetter<Speed>(typeof(Speed).GetProperty("Property7"));
            var prop8Setter = DelegateBuilder.BuildSetter<Speed>(typeof(Speed).GetProperty("Property8"));
            var prop9Setter = DelegateBuilder.BuildSetter<Speed>(typeof(Speed).GetProperty("Property9"));
            var prop10Setter = DelegateBuilder.BuildSetter<Speed>(typeof(Speed).GetProperty("Property10"));
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                prop1Setter(inst, "1");
                prop2Setter(inst, 2);
                prop3Setter(inst, 3m);
                prop4Setter(inst, new byte[0]);
                prop5Setter(inst, "5");
                prop6Setter(inst, 6);
                prop7Setter(inst, seven);
                prop8Setter(inst, eight);
                prop9Setter(inst, nine);
                prop10Setter(inst, Speed.SpeedEnum.None);
            }
            watch.Stop();
            inst.Property1.Should().Be.EqualTo("1");
            inst.Property2.Should().Be.EqualTo(2);
            inst.Property3.Should().Be.EqualTo(3m);
            inst.Property4.Should().Not.Be.Null();
            inst.Property5.Should().Be.EqualTo("5");
            inst.Property6.Should().Be.EqualTo(6);
            inst.Property7.Should().Be.EqualTo(7);
            inst.Property8.Should().Be.EqualTo(8);
            inst.Property9.Should().Be.EqualTo(9);
            inst.Property10.Should().Be.EqualTo(Speed.SpeedEnum.None);
            Trace.WriteLine(watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Property_DirectGetter_Speed()
        {
            Speed inst = new Speed();
            long seven = 7;
            double eight = 8;
            float nine = 9;
            var watch = Stopwatch.StartNew();

            inst.Property1 = "1";
            inst.Property2 = 2;
            inst.Property3 = 3m;
            inst.Property4 = new byte[0];
            inst.Property5 = "5";
            inst.Property6 = 6;
            inst.Property7 = seven;
            inst.Property8 = eight;
            inst.Property9 = nine;
            inst.Property10 = Speed.SpeedEnum.None;

            for (int i = 0; i < Iterations; i++)
            {
                var a = inst.Property1;
                var b = inst.Property2;
                var c = inst.Property3;
                var d = inst.Property4;
                var e = inst.Property5;
                var f = inst.Property6;
                var g = inst.Property7;
                var h = inst.Property8;
                var j = inst.Property9;
                var k = inst.Property10;
            }
            watch.Stop();
            inst.Property1.Should().Be.EqualTo("1");
            inst.Property2.Should().Be.EqualTo(2);
            inst.Property3.Should().Be.EqualTo(3m);
            inst.Property4.Should().Not.Be.Null();
            inst.Property5.Should().Be.EqualTo("5");
            inst.Property6.Should().Be.EqualTo(6);
            inst.Property7.Should().Be.EqualTo(7);
            inst.Property8.Should().Be.EqualTo(8);
            inst.Property9.Should().Be.EqualTo(9);
            inst.Property10.Should().Be.EqualTo(Speed.SpeedEnum.None);
            Trace.WriteLine(watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void DelegateBuilder_PropertyGetter_Speed()
        {
            Speed inst = new Speed();
            long seven = 7;
            double eight = 8;
            float nine = 9;
            var prop1Getter = DelegateBuilder.BuildGetter<Speed>(typeof(Speed).GetProperty("Property1"));
            var prop2Getter = DelegateBuilder.BuildGetter<Speed>(typeof(Speed).GetProperty("Property2"));
            var prop3Getter = DelegateBuilder.BuildGetter<Speed>(typeof(Speed).GetProperty("Property3"));
            var prop4Getter = DelegateBuilder.BuildGetter<Speed>(typeof(Speed).GetProperty("Property4"));
            var prop5Getter = DelegateBuilder.BuildGetter<Speed>(typeof(Speed).GetProperty("Property5"));
            var prop6Getter = DelegateBuilder.BuildGetter<Speed>(typeof(Speed).GetProperty("Property6"));
            var prop7Getter = DelegateBuilder.BuildGetter<Speed>(typeof(Speed).GetProperty("Property7"));
            var prop8Getter = DelegateBuilder.BuildGetter<Speed>(typeof(Speed).GetProperty("Property8"));
            var prop9Getter = DelegateBuilder.BuildGetter<Speed>(typeof(Speed).GetProperty("Property9"));
            var prop10GGetter = DelegateBuilder.BuildGetter<Speed>(typeof(Speed).GetProperty("Property10"));
            inst.Property1 = "1";
            inst.Property2 = 2;
            inst.Property3 = 3m;
            inst.Property4 = new byte[0];
            inst.Property5 = "5";
            inst.Property6 = 6;
            inst.Property7 = seven;
            inst.Property8 = eight;
            inst.Property9 = nine;
            inst.Property10 = Speed.SpeedEnum.None;
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                var a = prop1Getter(inst);
                var b = prop2Getter(inst);
                var c = prop3Getter(inst);
                var d = prop4Getter(inst);
                var e = prop5Getter(inst);
                var f = prop6Getter(inst);
                var g = prop7Getter(inst);
                var h = prop8Getter(inst);
                var j = prop9Getter(inst);
                var k = prop10GGetter(inst);
            }
            watch.Stop();
            inst.Property1.Should().Be.EqualTo("1");
            inst.Property2.Should().Be.EqualTo(2);
            inst.Property3.Should().Be.EqualTo(3m);
            inst.Property4.Should().Not.Be.Null();
            inst.Property5.Should().Be.EqualTo("5");
            inst.Property6.Should().Be.EqualTo(6);
            inst.Property7.Should().Be.EqualTo(7);
            inst.Property8.Should().Be.EqualTo(8);
            inst.Property9.Should().Be.EqualTo(9);
            inst.Property10.Should().Be.EqualTo(Speed.SpeedEnum.None);
            Trace.WriteLine(watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Reflection_PropertySetter_Speed()
        {
            Speed inst = new Speed();
            long seven = 7;
            double eight = 8;
            float nine = 9;
            var prop1Setter = typeof(Speed).GetProperty("Property1");
            var prop2Setter = typeof(Speed).GetProperty("Property2");
            var prop3Setter = typeof(Speed).GetProperty("Property3");
            var prop4Setter = typeof(Speed).GetProperty("Property4");
            var prop5Setter = typeof(Speed).GetProperty("Property5");
            var prop6Setter = typeof(Speed).GetProperty("Property6");
            var prop7Setter = typeof(Speed).GetProperty("Property7");
            var prop8Setter = typeof(Speed).GetProperty("Property8");
            var prop9Setter = typeof(Speed).GetProperty("Property9");
            var prop10Setter = typeof(Speed).GetProperty("Property10");
            var watch = Stopwatch.StartNew();

            for (int i = 0; i < Iterations; i++)
            {
                prop1Setter.SetValue(inst, "1", null);
                prop2Setter.SetValue(inst, 2, null);
                prop3Setter.SetValue(inst, 3m, null);
                prop4Setter.SetValue(inst, new byte[0], null);
                prop5Setter.SetValue(inst, "5", null);
                prop6Setter.SetValue(inst, 6, null);
                prop7Setter.SetValue(inst, seven, null);
                prop8Setter.SetValue(inst, eight, null);
                prop9Setter.SetValue(inst, nine, null);
                prop10Setter.SetValue(inst, Speed.SpeedEnum.None, null);
            }
            watch.Stop();
            inst.Property1.Should().Be.EqualTo("1");
            inst.Property2.Should().Be.EqualTo(2);
            inst.Property3.Should().Be.EqualTo(3m);
            inst.Property4.Should().Not.Be.Null();
            inst.Property5.Should().Be.EqualTo("5");
            inst.Property6.Should().Be.EqualTo(6);
            inst.Property7.Should().Be.EqualTo(7);
            inst.Property8.Should().Be.EqualTo(8);
            inst.Property9.Should().Be.EqualTo(9);
            inst.Property10.Should().Be.EqualTo(Speed.SpeedEnum.None);
            Trace.WriteLine(watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Enumeration_Tests()
        {
            var objectClassesDictionary = new Dictionary<string, string>
            {
                {"top", ""},
                {"person", ""},
                {"organizationalPerson", ""},
                {"user", ""},
            };
            var objectClasses = objectClassesDictionary.Values.ToArray();
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < int.Parse("100,000", NumberStyles.AllowThousands); i++)
            {
                foreach (var objectClass in objectClasses)
                {
                    objectClassesDictionary.ContainsKey(objectClass);
                }
            }
            watch.Stop();
            Trace.WriteLine(watch.ElapsedMilliseconds);
        }
    }
}