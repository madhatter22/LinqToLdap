using System.Collections.Generic;
using LinqToLdap.Mapping;

namespace LinqToLdap.Tests.ClassMapAssembly
{
    public struct SomeStruct
    {
    }

    public interface ISomethingToThrowOff
    {
    }

    public class AssemblyTestClass
    {
        public string Property1 { get; set; }
    }

    [DirectorySchema("context", ObjectClasses = new[] { "AssenblyTestClass2" })]
    public class AssenblyTestClass2
    {
        [DirectoryAttribute]
        public string Property1 { get; set; }
    }

    public class AssemblyTestClassSub : AssemblyTestClass
    {
        public string Property2 { get; set; }
    }

    public class AssemblyTestClassMap : ClassMap<AssemblyTestClass>
    {
        public override IClassMap PerformMapping(string namingContext = null, string objectCategory = null, 
            bool includeObjectCategory = true, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true)
        {
            NamingContext("name");

            ObjectClasses(new[] { GetType().Name });

            Map(p => p.Property1);

            return this;
        }
    }

    public class AssemblyTestClassSubMap : SubClassMap<AssemblyTestClassSub, AssemblyTestClass>
    {
        public AssemblyTestClassSubMap()
            : base(new AssemblyTestClassMap())
        {
        }

        public override IClassMap PerformMapping(string namingContext = null, string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true)
        {
            NamingContext("name2");

            ObjectClasses(new[] { GetType().Name });

            Map(p => p.Property2);

            return this;
        }
    }
}
