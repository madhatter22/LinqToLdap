using LinqToLdap.Async;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using System;
using System.DirectoryServices.Protocols;
using System.Linq;

namespace LinqToLdap.Tests
{
    [TestClass]
    public class AsyncIntegrationTests
    {
        private LdapConfiguration _configuration;
        private IDirectoryContext _context;
        private const string ServerName = "localhost";

        [TestInitialize]
        public void SetUp()
        {
            _configuration = new LdapConfiguration()
                .AddMapping(new IntegrationUserTestMapping(), IntegrationUserTest.NamingContext, new[] { "user" })
                .AddMapping(new AttributeClassMap<IntegrationGroupTest>(), IntegrationGroupTest.NamingContext, new[] { "top", "group" }, true, "group")
                .AddMapping(new AttributeClassMap<PersonInheritanceTest>())
                .AddMapping(new AttributeClassMap<OrgPersonInheritanceTest>())
                .AddMapping(new AttributeClassMap<UserInheritanceTest>())
                .AddMapping(new AttributeClassMap<PersonCatchAllTest>())
                .AddMapping(new AttributeClassMap<OrgPersonCatchAllTest>())
                .MaxPageSizeIs(1000)
                .LogTo(new SimpleTextLogger(Console.Out));

            _configuration.ConfigureFactory(ServerName)
                //.AuthenticateBy(AuthType.Basic)
                //.AuthenticateAs(new NetworkCredential("CN=AlphaUser,CN=Users,CN=Employees,DC=Northwind,DC=local", "test"))
                .AuthenticateBy(AuthType.Negotiate)
                ;

            _context = _configuration.CreateContext();
        }

        [TestCleanup]
        public void TearDown()
        {
            _context.Dispose();
        }

#if !NET35 && !NET40

        [TestMethod]
        public void AnyAsync_returns_same_result_as_Any()
        {
            //arrange
            var any = _context.Query<PersonInheritanceTest>().Any();
            var anyPredicate = _context.Query<PersonInheritanceTest>().Any(x => x.CommonName.StartsWith("X"));

            //act
            var anyAsnyc = _context.Query<PersonInheritanceTest>().AnyAsync().Result;
            var anyPredicateAsnyc = _context.Query<PersonInheritanceTest>().AnyAsync(x => x.CommonName.StartsWith("X")).Result;

            //assert
            any.Should().Be.EqualTo(anyAsnyc);
            anyPredicate.Should().Be.EqualTo(anyPredicateAsnyc);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Can_Query_Accross_Threads()
        {
            _context.FieldValueEx<LdapConnection>("_connection").Bind();
            Action work = () =>
            {
                for (int i = 0; i < 50; i++)
                {
                    _context.Query(IntegrationGroupTest.NamingContext).ToList();
                }
            };

            Executing.This(
                () =>
                    System.Threading.Tasks.Task.WaitAll(System.Threading.Tasks.Task.Factory.StartNew(work),
                        System.Threading.Tasks.Task.Factory.StartNew(work),
                        System.Threading.Tasks.Task.Factory.StartNew(work)))
                .Should()
                .NotThrow();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ToListAsync_Executes()
        {
            //act
            var task = _context.Query<PersonInheritanceTest>()
                .ToListAsync();
            task.Wait();

            //assert
            task.Result.Should().Have.Count.GreaterThan(1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void CountAsync_Executes()
        {
            //act
            var countWithout = _context.Query<PersonInheritanceTest>()
                .CountAsync().Result;
            var countWith = _context.Query<PersonInheritanceTest>()
                .CountAsync(x => Filter.StartsWith(x, "sn", "J", false)).Result;

            //assert
            countWithout.Should().Be.GreaterThan(1);
            countWith.Should().Be.GreaterThan(1).And.Be.LessThan(countWithout);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void LongCountAsync_Executes()
        {
            //act
            var countWithout = _context.Query<PersonInheritanceTest>()
                .LongCountAsync().Result;
            var countWith = _context.Query<PersonInheritanceTest>()
                .LongCountAsync(x => Filter.StartsWith(x, "sn", "J", false)).Result;

            //assert
            countWithout.Should().Be.GreaterThan(1);
            countWith.Should().Be.GreaterThan(1).And.Be.LessThan(countWithout);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void FirstOrDefaultAsync_Executes()
        {
            //act
            var task = _context.Query<PersonInheritanceTest>().FirstOrDefaultAsync();
            task.Wait();

            //assert
            task.Result.Should().Not.Be.Null();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void FirstOrDefaultAsync_WithPredicate_Executes()
        {
            //act
            var task = _context.Query<IntegrationGroupTest>().FirstOrDefaultAsync(g => g.Member != null);
            task.Wait();

            //assert
            task.Result.CommonName.Should().Be.EqualTo("RangeTest");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void FirstAsync_Executes()
        {
            //act
            var task = _context.Query<PersonInheritanceTest>().FirstAsync();
            task.Wait();

            //assert
            task.Result.Should().Not.Be.Null();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void FirstAsync_WithPredicate_Executes()
        {
            //act
            var task = _context.Query<IntegrationGroupTest>().FirstAsync(g => g.Member != null);
            task.Wait();

            //assert
            task.Result.CommonName.Should().Be.EqualTo("RangeTest");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SingleAsync_Executes()
        {
            //act
            var task = _context.Query(PersonInheritanceTest.NamingContext, SearchScope.Base)
                .SingleAsync();
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(PersonInheritanceTest.NamingContext);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SingleAsync_WithPredicate_Executes()
        {
            //act
            var task = _context.Query(PersonInheritanceTest.NamingContext)
                .SingleAsync(g => Filter.Equal(g, "distinguishedName", PersonInheritanceTest.NamingContext, false));
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(PersonInheritanceTest.NamingContext);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SingleOrDefaultAsync_Executes()
        {
            //act
            var task = _context.Query(PersonInheritanceTest.NamingContext, SearchScope.Base)
                .SingleOrDefaultAsync();
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(PersonInheritanceTest.NamingContext);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SingleOrDefaultAsync_WithPredicate_Executes()
        {
            //act
            var task = _context.Query(PersonInheritanceTest.NamingContext)
                .SingleOrDefaultAsync(g => Filter.Equal(g, "distinguishedName", PersonInheritanceTest.NamingContext, false));
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(PersonInheritanceTest.NamingContext);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ListServerAttributesAsync_Executes()
        {
            //act
            var task = _context.Query(PersonInheritanceTest.NamingContext, SearchScope.Base)
                .ListAttributesAsync();
            task.Wait();

            //assert
            task.Result.Should().Have.Count.EqualTo(1);
            task.Result.First().Value.Should().Have.Count.GreaterThan(1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ListServerAttributesAsync_WithSpecifiedAttributes_Executes()
        {
            //act
            var task = _context.Query(PersonInheritanceTest.NamingContext, SearchScope.Base)
                .ListAttributesAsync(new[] { "cn" });
            task.Wait();

            //assert
            task.Result.Should().Have.Count.EqualTo(1);
            task.Result.First().Value.Satisfies(kvp => kvp.Count() == 1 && kvp.First().Key == "cn");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetByDNAsync_MappedObject_Executes()
        {
            Assert.Fail();
            //act
            var task = _context.GetByDNAsync<PersonInheritanceTest>(PersonInheritanceTest.NamingContext);
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(PersonInheritanceTest.NamingContext);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetByDNAsync_UnmappedObject_Executes()
        {
            Assert.Fail();
            //act
            var task = _context.GetByDNAsync(PersonInheritanceTest.NamingContext);
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(PersonInheritanceTest.NamingContext);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetByDNAsync_LdapConnectionExtension_Executes()
        {
            Assert.Fail();
            //act
            var task = _context.FieldValueEx<LdapConnection>("_connection")
                .GetByDNAsync(PersonInheritanceTest.NamingContext);
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(PersonInheritanceTest.NamingContext);
        }

#endif
    }
}