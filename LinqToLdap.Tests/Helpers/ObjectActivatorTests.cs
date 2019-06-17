using LinqToLdap.Collections;
using LinqToLdap.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;

namespace LinqToLdap.Tests.Helpers
{
    [TestClass]
    public class ObjectActivatorTests
    {
        private const int Iterations = 5000 * 100;

        [TestMethod]
        public void CreateGenericInstance_performance()
        {
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                var parameters = new object[]
                                 {
                                     100,
                                     new byte[0],
                                     new string[0],
                                     ""
                                 };
                var inst = ObjectActivator.CreateGenericInstance(typeof(LdapPage<>), typeof(string), parameters, null);
            }
            watch.Stop();
            Trace.WriteLine(watch.ElapsedMilliseconds);
        }

        [TestMethod]
        public void Activate_performance()
        {
            var watch = Stopwatch.StartNew();
            for (int i = 0; i < Iterations; i++)
            {
                var parameters = new object[]
                                 {
                                     100,
                                     new byte[0],
                                     new string[0],
                                     ""
                                 };
                Activator.CreateInstance(typeof(LdapPage<string>), parameters);
            }
            watch.Stop();
            Trace.WriteLine(watch.ElapsedMilliseconds);
        }
    }
}