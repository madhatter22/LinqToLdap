using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Principal;
using System.Text;
using LinqToLdap.Collections;
using LinqToLdap.EventListeners;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

#if (!NET35)

using LinqToLdap.Contrib;

#endif

namespace LinqToLdap.Tests
{
    [DirectorySchema(NamingContext, ObjectClasses = new[] { "top", "person" })]
    public class PersonInheritanceTest : DirectoryObjectBase
    {
        protected const string NamingContext = "CN=InheritanceTest,CN=Employees,DC=Northwind,DC=local";

        [DistinguishedName]
        public string DistinguishedName { get; set; }

        [DirectoryAttribute("sn")]
        public string LastName { get; set; }

        [DirectoryAttribute("whenchanged", StoreGenerated = true)]
        public DateTime? LastChanged { get; set; }

        [DirectoryAttribute("objectsid", StoreGenerated = true)]
        public SecurityIdentifier Sid { get; set; }

        [DirectoryAttribute("usnchanged", StoreGenerated = true)]
        public int Version { get; set; }

        [DirectoryAttribute("objectguid", StoreGenerated = true)]
        public Guid Guid { get; set; }

        [DirectoryAttribute("cn", ReadOnly = true)]
        public string CommonName { get; set; }
    }

    [DirectorySchema(NamingContext, ObjectClasses = new[] { "top", "person", "organizationalPerson" })]
    public class OrgPersonInheritanceTest : PersonInheritanceTest
    {
        [DirectoryAttribute]
        public string Title { get; set; }

        [DirectoryAttribute]
        public string PostalCode { get; set; }

        [DirectoryAttribute(StoreGenerated = true)]
        public DateTime? WhenCreated { get; set; }

        [DirectoryAttribute("givenname")]
        public string FirstName { get; set; }

        [DirectoryAttribute("l")]
        public string City { get; set; }

        [DirectoryAttribute("c")]
        public string Country { get; set; }

        [DirectoryAttribute("employeeid")]
        public long EmployeeId { get; set; }

        [DirectoryAttribute("telephonenumber")]
        public string PhoneNumber { get; set; }

        [DirectoryAttribute]
        public string Street { get; set; }

        [DirectoryAttribute]
        public string Comment { get; set; }
    }

    [DirectorySchema(NamingContext, ObjectClasses = new[] { "top", "person", "organizationalPerson", "user" })]
    public class UserInheritanceTest : OrgPersonInheritanceTest
    {
        [DirectoryAttribute("badpwdcount", StoreGenerated = true)]
        public int BadPasswordCount { get; set; }

        [DirectoryAttribute(DateTimeFormat = null)]
        public DateTime? AccountExpires { get; set; }
    }

    [DirectorySchema(NamingContext, ObjectClasses = new[] { "top", "person" })]
    public class PersonCatchAllTest : DirectoryObjectBase
    {
        protected const string NamingContext = "CN=InheritanceTest,CN=Employees,DC=Northwind,DC=local";

        [DistinguishedName]
        public string DistinguishedName { get; set; }

        [DirectoryAttribute("cn", ReadOnly = true)]
        public string CommonName { get; set; }

        public IDirectoryAttributes CatchAll { get; set; }
    }

    [DirectorySchema(NamingContext, ObjectClasses = new[] { "top", "person", "organizationalPerson" })]
    public class OrgPersonCatchAllTest : PersonCatchAllTest
    {
        [DirectoryAttribute]
        public string Title { get; set; }
    }

    public class IntegrationUserTest
    {
        public const string NamingContext = "CN=Users2,CN=Employees,DC=Northwind,DC=local";
        public const string NamingContext2 = "CN=Users,CN=Employees,DC=Northwind,DC=local";

        public string DistinguishedName { get; set; }
        public string Cn { get; set; }
        public Guid ObjectGuid { get; set; }
        public string Manager { get; set; }
        public string[] Employees { get; set; }
        public DateTime? AccountExpires { get; set; }
        public DateTime WhenCreated { get; set; }
        public SecurityIdentifier ObjectSid { get; set; }
        public string Mail { get; set; }
        public string[] MemberOf { get; set; }

        public string GetDistinguishedName()
        {
            return "CN=" + Cn + "," + NamingContext;
        }
    }

    [DirectorySchema(NamingContext, ObjectClass = "group")]
    public class IntegrationGroupTest : DirectoryObjectBase
    {
        public const string NamingContext = "CN=Roles,CN=Employees,DC=Northwind,DC=local";

        [DistinguishedName]
        public string DistinguishedName { get; set; }

        [DirectoryAttribute("cn", ReadOnly = true)]
        public string CommonName { get; private set; }

        [DirectoryAttribute]
        public Collection<String> Member
        {
            get; set;
        }

        public void SetDistinguishedName(string cn)
        {
            CommonName = cn;
            DistinguishedName = "CN=" + cn + "," + NamingContext;
        }
    }

    public class IntegrationUserTestMapping : ClassMap<IntegrationUserTest>
    {
        public override IClassMap PerformMapping(string namingContext = null, string objectCategory = null, bool includeObjectCategory = true, IEnumerable<string> objectClasses = null, bool includeObjectClasses = true)
        {
            ObjectCategory(objectCategory);
            ObjectClasses(objectClasses);
            NamingContext(namingContext);

            DistinguishedName(c => c.DistinguishedName);
            Map(c => c.Cn)
                .ReadOnly();

            Map(c => c.Mail);
            Map(c => c.ObjectGuid)
                .StoreGenerated();
            Map(c => c.Manager)
                .Named("manager");
            Map(c => c.Employees)
                .Named("directreports")
                .ReadOnly();
            Map(c => c.AccountExpires)
                .Named("accountExpires")
                .DateTimeFormat(null)
                .DirectoryValue("9223372036854775807").Returns(null)
                .InstanceValue(DateTime.MinValue).Sends("9223372036854775807")
                .InstanceValueNullOrDefault().Sends("9223372036854775807");

            Map(c => c.WhenCreated)
                .StoreGenerated();

            Map(c => c.ObjectSid)
                .StoreGenerated();

            return this;
        }
    }

    [TestClass]
    public class IntegrationTests : IPreAddEventListener, IPostAddEventListener, IPreUpdateEventListener, IPostUpdateEventListener, IPreDeleteEventListener, IPostDeleteEventListener
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

        [TestMethod]
        [TestCategory("Integration")]
        public void Can_Add_Update_Remove_Dynamic()
        {
            var attributes = new DirectoryAttributes("CN=IntegrationTest," + IntegrationUserTest.NamingContext);
            attributes.SetNull("AccountExpires");
            attributes.Set("objectclass", "user");

            var added = _context.AddAndGet(attributes);
            added.Should().Not.Be.Null();

            added.GetString("cn").Should().Be.EqualTo("IntegrationTest");
            added.GetString("accountexpires").Should().Be.Null();
            added.GetGuid("objectguid").Should().Have.Value();
            added.GetSecurityIdentifier("objectsid").Should().Not.Be.Null();
            added.GetSecurityIdentifiers("objectsid").Should().Not.Be.Empty();
            added.GetStrings("objectclass").Should().Have.Count.GreaterThan(1);

            added.Set("accountExpires", "9223372036854775807").SetNull("manager");

            added = _context.UpdateAndGet(added);

            added.GetString("accountExpires").Should().Be.EqualTo("9223372036854775807");
            added.GetDateTime("accountExpires", null).Should().Not.Have.Value();
            added.GetString("manager").Should().Be.Null();

            var renamed = _context.RenameEntry(added.DistinguishedName, "IntegrationTest2");

            var moved = _context.MoveEntry(renamed, IntegrationUserTest.NamingContext2);

            _context.Delete(moved);

            Executing.This(() => _context.GetByDN(moved))
                .Should().Throw<DirectoryOperationException>().And.Exception.Message
                .Should().Contain("does not exist");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Can_Add_Update_Remove_Static()
        {
            var test = new IntegrationUserTest
            {
                Cn = "IntegrationTest",
                AccountExpires = DateTime.MinValue
            };

            var added = _context.AddAndGet(test, test.GetDistinguishedName());

            added.Should().Not.Be.Null();
            added.AccountExpires.Should().Not.Have.Value();
            added.Cn.Should().Be.EqualTo(test.Cn);
            added.AccountExpires.Should().Not.Have.Value();
            added.ObjectGuid.Should().Not.Be.Null().And.Not.Be.EqualTo(default(Guid));
            added.ObjectSid.Should().Not.Be.Null();
            added.Manager.Should().Be.Null();
            added.Employees.Should().Be.Null();
            added.WhenCreated.Should().Be.GreaterThan(DateTime.Now.Subtract(TimeSpan.FromHours(1)));

            var queryResult = _context.Query<IntegrationUserTest>()
                .Where(u => u.ObjectGuid == added.ObjectGuid)
                .Select(u => u.ObjectGuid)
                .FirstOrDefault();

            queryResult.Should().Be.EqualTo(added.ObjectGuid);

            var now = DateTime.Now;
            added.AccountExpires = now;

            added = _context.UpdateAndGet(added);

            added.AccountExpires.Should().Be.EqualTo(now);

            _context.Delete(added.DistinguishedName);

            Executing.This(() => _context.GetByDN(added.DistinguishedName))
                .Should().Throw<DirectoryOperationException>().And.Exception.Message
                .Should().Contain("does not exist");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetByDN_LoadsObject()
        {
            _context.GetByDN("CN=TestUser,CN=Users2,CN=Employees,DC=Northwind,DC=local").Should().Not.Be.Null();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Can_Add_Update_ModifyMembers_Group()
        {
            var members = _context.Query<IntegrationUserTest>().Select(u => u.DistinguishedName).ToList();

            members.Count.Should().Be.GreaterThan(0);
            var group = new IntegrationGroupTest
            {
                Member = new Collection<string>(members.GetRange(0, members.Count - 1))
            };

            group.SetDistinguishedName("TestGroup");

            var added = _context.AddAndGet(group);

            var addedQueried = _context.Query<IntegrationGroupTest>()
                .Where(c => c.DistinguishedName == added.DistinguishedName)
                .Select(c => c)
                .Single();

            added.OriginalValues["CommonName"].Should().Be.EqualTo("TestGroup");
            added.OriginalValues["Member"].As<Collection<string>>().Should().Have.Count.EqualTo(members.Count - 1);
            added.OriginalValues["distinguishedname"].Should().Be.EqualTo(added.DistinguishedName);

            added.OriginalValues["CommonName"].Should().Be.EqualTo(addedQueried.OriginalValues["CommonName"]);
            added.OriginalValues["Member"].As<Collection<string>>()
                .Should().Have.SameSequenceAs(addedQueried.OriginalValues["Member"].As<Collection<string>>());
            added.OriginalValues["distinguishedname"].Should()
                .Be.EqualTo(addedQueried.OriginalValues["distinguishedname"]);

            foreach (var member in members.GetRange(0, members.Count - 1))
            {
                added.Member.Should().Contain(member);
            }

            int removedIndex = members.IndexOf(added.Member[0]);
            added.Member.Should().Not.Contain(members.Last());
            added.Member.RemoveAt(0);
            added.Member.Add(members.Last());

            var updated = _context.UpdateAndGet(added);

            updated.Should().Not.Be.EqualTo(added);

            updated.Member.Count.Should().Be.EqualTo(members.Count - 1);
            updated.Member.Should().Not.Contain(members[removedIndex]);
            updated.Member.Should().Contain(members.Last());

            updated.OriginalValues["CommonName"].Should().Be.EqualTo("TestGroup");
            updated.OriginalValues["distinguishedname"].Should().Be.EqualTo(added.DistinguishedName);

            updated.OriginalValues["CommonName"].Should().Be.EqualTo(addedQueried.OriginalValues["CommonName"]);
            updated.OriginalValues["Member"].As<Collection<string>>()
                .Should().Have.SameSequenceAs(updated.Member);
            updated.OriginalValues["distinguishedname"].Should()
                .Be.EqualTo(addedQueried.OriginalValues["distinguishedname"]);

            _context.Delete(updated.DistinguishedName);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Can_Add_Update_ModifyMembersDynamic_Group()
        {
            var members = _context.Query<IntegrationUserTest>().Select(u => u.DistinguishedName).ToList();
            members.Count.Should().Be.GreaterThan(0);

            var group = new DirectoryAttributes("CN=TestGroup,CN=Roles,CN=Employees,DC=Northwind,DC=local");

            group.Set("objectClass", new[] { "top", "group" });
            group.Set("Member", new Collection<string>(members.GetRange(0, members.Count - 1)));

            var added = _context.AddAndGet(group);

            var addedMembers = added.GetStrings("member").ToList();

            foreach (var member in members.GetRange(0, members.Count - 1))
            {
                addedMembers.Should().Contain(member);
            }

            int removedIndex = members.IndexOf(addedMembers[0]);
            addedMembers.Should().Not.Contain(members.Last());
            addedMembers.RemoveAt(0);
            addedMembers.Add(members.Last());

            added.Set("member", addedMembers);

            var updated = _context.UpdateAndGet(added);

            updated.GetStrings("member").Should().Not.Contain(members[removedIndex])
                .And.Contain(members.Last());

            _context.Delete(updated.DistinguishedName);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RenameEntry_Container_RenamesEntryAndAllEntriesWithin()
        {
            string dn = "CN=Users2,CN=Employees,DC=Northwind,DC=local";
            var countResult = _context.Query(dn).Count(da => !Filter.Equal(da, "objectClass", "container", false));
            countResult.Should().Be.GreaterThanOrEqualTo(4);
            var newDn = _context.RenameEntry(dn, "Users Rename");
            _context.Query(newDn).Count(da => !Filter.Equal(da, "objectClass", "container", false)).Should().Be.GreaterThanOrEqualTo(4);
            _context.RenameEntry(newDn, "Users2");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ListServerAttributes()
        {
            var attributes = _context.ListServerAttributes();
            attributes.Should().Not.Be.Null().And.Not.Be.Empty();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void AttributeScopedQuery()
        {
            var response2 = _context.Query<IntegrationUserTest>(SearchScope.Base, "CN=RangeTest,CN=Roles,CN=Employees,DC=Northwind,DC=local")
                .IgnoreOC()
                .ScopedToAttribute("member")
                .Take(2)
                .ToList();

            response2.Count.Should().Be.EqualTo(2);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ToArray_Works()
        {
            _context.Query<IntegrationUserTest>().ToArray().Should().Not.Be.Null().And.Not.Be.Empty();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void DeleteSubTree()
        {
            var container = new DirectoryAttributes("CN=DeleteContainer,CN=Employees,DC=Northwind,DC=local");
            container.Set("objectClass", new[] { "top", "container" });

            _context.Add(container);

            var attributes = new DirectoryAttributes("CN=IntegrationTest," + container.DistinguishedName);
            attributes.SetNull("AccountExpires");
            attributes.Set("objectclass", "user");

            _context.AddAndGet(attributes);

            _context.Delete(container.DistinguishedName, new TreeDeleteControl());

            Executing.This(() => _context.GetByDN(container.DistinguishedName))
                .Should().Throw<DirectoryOperationException>().And.Exception.Message
                .Should().Contain("does not exist");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RetrieveRanges_NoStartSpecified_GetsRanges()
        {
            //prepare
            string attribute = "member";
            string dn = "CN=RangeTest,CN=Roles,CN=Employees,DC=Northwind,DC=local";

            //act
            var group = _context.RetrieveRanges<string>(dn, attribute);

            //assert
            group.Should().Have.Count.GreaterThan(2000);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RetrieveRanges_StartAt3000_GetsRanges()
        {
            //prepare
            string attribute = "member";
            string dn = "CN=RangeTest,CN=Roles,CN=Employees,DC=Northwind,DC=local";

            //act
            var group = _context.RetrieveRanges<string>(dn, attribute, 3000);

            //assert
            group.Should().Have.Count.GreaterThan(5000).And.Be.LessThan(10000);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void WithoutPaging_LargeResultSet_ThrowsException()
        {
            //act
            Executing.This(() => _context.Query("CN=Users,CN=Employees,DC=Northwind,DC=local").WithoutPaging().ToList())
                .Should().Throw<DirectoryOperationException>().And.Exception.Message
                .Should().Be.EqualTo("The size limit was exceeded");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Query_FromLdapConnection_Works()
        {
            using (var connection = _configuration.ConnectionFactory.GetConnection())
            {
                var users = connection.Query("CN=Users2,CN=Employees,DC=Northwind,DC=local", log: _configuration.Log)
                    .Select("distinguishedName")
                    .ToList();

                users.Should().Not.Be.Empty();
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Can_PageQuery_Manually()
        {
            var page = _context.Query("CN=Users,CN=Employees,DC=Northwind,DC=local")
                            .WithControls(new[]
                                              {
                                                  new PageResultRequestControl {PageSize = 1000}
                                              })
                            .ToList() as LdapPage<IDirectoryAttributes>;

            var list = new List<IDirectoryAttributes>(page);

            while (page.HasNextPage && (list.Count == 0 || list.Count < 5000))
            {
                page = _context.Query("CN=Users,CN=Employees,DC=Northwind,DC=local")
                            .WithControls(new[]
                                              {
                                                  new PageResultRequestControl {PageSize = 1000, Cookie = page.NextPage}
                                              })
                            .ToList() as LdapPage<IDirectoryAttributes>;

                list.AddRange(page);
            }

            list.Should().Have.Count.GreaterThan(1000);
        }

        [TestMethod]
        [TestCategory("Integration")]
        [Ignore]
        public void Can_Reset_And_Change_Password()
        {
            var connectionFactory = new LdapConnectionFactory(ServerName);
            connectionFactory
                //.AuthenticateBy(AuthType.Basic)
                //.AuthenticateAs(new NetworkCredential("CN=AlphaUser,CN=Users,CN=Employees,DC=Northwind,DC=local", "test"))
                .AuthenticateBy(AuthType.Negotiate);

            using (var connection = connectionFactory.GetConnection())
            {
                var user = connection.Query("CN=Employees,DC=Northwind,DC=local")
                    .Select("distinguishedname")
                    .FirstOrDefault(da => Filter.Equal(da, "cn", "TestUser2", false));

                var newPassword = GetPasswordData("my pass");
                user.Set("unicodePwd", newPassword);

                connection.Update(user, _configuration.Log);
            }

            using (var connection = connectionFactory.GetConnection())
            {
                var user = connection.Query("CN=Employees,DC=Northwind,DC=local")
                    .Select("distinguishedname")
                    .FirstOrDefault(da => Filter.Equal(da, "cn", "TestUser2", true));

                var currentPassword = GetPasswordData("my pass");
                var newPassword = GetPasswordData("new pass");

                user.AddModification(currentPassword.ToDirectoryModification("unicodePwd", DirectoryAttributeOperation.Delete));
                user.AddModification(newPassword.ToDirectoryModification("unicodePwd", DirectoryAttributeOperation.Add));

                connection.Update(user, _configuration.Log);
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Test_Group_With_No_MembersCanUpdateMembers()
        {
            //prepare
            var readerGroup = _context.Query<IntegrationGroupTest>()
                .FirstOrDefault(c => c.CommonName == "Readers");
            readerGroup.Should().Not.Be.Null();
            readerGroup.Member.Should().Be.Null();
            var user = _context.Query<IntegrationUserTest>().FirstOrDefault();
            user.Should().Not.Be.Null();
            readerGroup.Member = new Collection<string>(new List<string> { user.DistinguishedName });

            //act
            var updated = _context.UpdateAndGet(readerGroup);

            //assert
            updated.Should().Not.Be.SameInstanceAs(readerGroup);
            updated.Member.Should().Contain(user.DistinguishedName);

            //prepare
            updated.Member.Clear();

            //act
            var updatedAgain = _context.UpdateAndGet(updated);

            //assert
            updatedAgain.Should().Not.Be.SameInstanceAs(updated);
            updatedAgain.Member.Should().Be.Null();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Virtual_List_View_Test()
        {
            int skip = 0;
            int take = 700;

            var results = _context.Query<IntegrationUserTest>(SearchScope.Subtree, IntegrationUserTest.NamingContext2)
                .Where(x => Filter.StartsWith(x, "givenName", "T", true))
                .OrderByDescending(x => x.Cn)
                .Skip(skip)
                .Take(700)
                .ToList();

            results.Should().Not.Be.Empty().And.Have.Count.EqualTo(700);
            results.As<IVirtualListView<IntegrationUserTest>>().Should().Not.Be.Null();
            results.As<IVirtualListView<IntegrationUserTest>>().ContextId
                .Should().Not.Be.Null();
            results.As<IVirtualListView<IntegrationUserTest>>().TargetPosition
                .Should().Be.GreaterThanOrEqualTo(0);

            string[] attribs = { "cn", "sn", "givenName" };

            SearchRequest searchRequest = new SearchRequest
                                                    (IntegrationUserTest.NamingContext2,
                                                     "(givenName=T*)",
                                                     SearchScope.Subtree,
                                                     attribs);

            SortRequestControl sortRequest = new SortRequestControl("cn", true);

            searchRequest.Controls.Add(sortRequest);

            VlvRequestControl vlvRequest =
                new VlvRequestControl(0, take - 1, skip + 1);

            // add the vlv request to the searchRequest object
            searchRequest.Controls.Add(vlvRequest);

            SearchResponse searchResponse =
                (SearchResponse)_context.SendRequest(searchRequest);

            searchResponse.Should().Not.Be.Null();
            searchResponse.Entries.Count.Should().Be.GreaterThan(0);
            results.As<IVirtualListView<IntegrationUserTest>>().ContentCount
                .Should().Be.EqualTo(searchResponse.Controls[1].As<VlvResponseControl>().ContentCount);

            results.Select(x => x.DistinguishedName)
                .OrderBy(x => x)
                .Should()
                .Have.SameSequenceAs(
                    searchResponse.Entries.Cast<SearchResultEntry>().Select(x => x.DistinguishedName).OrderBy(x => x));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Test_Inheritance()
        {
            var people = _context.Query<PersonInheritanceTest>().ToList();

            people.Any(x => x is OrgPersonInheritanceTest).Should().Be.True();
            people.Any(x => x is UserInheritanceTest).Should().Be.True();

            var utcNow = DateTime.UtcNow.AddDays(50);
            var user = people.OfType<UserInheritanceTest>().Skip(1).First();
            user.City.Should().Not.Be.NullOrEmpty();
            user.Title.Should().Not.Be.NullOrEmpty();
            user.AccountExpires.Satisfies(x => x == null || x < utcNow);
            user.AccountExpires = utcNow;
            _context.Update(user);

            var updatedUser =
                _context.Query<PersonInheritanceTest>().FirstOrDefault(x => x.CommonName == user.CommonName);

            updatedUser.Should().Not.Be.Null();
            updatedUser.As<UserInheritanceTest>().AccountExpires.Should().Have.Value();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Test_Inheritance_GetByDn_And_Update()
        {
            var people = _context.Query<PersonInheritanceTest>().Take(100).ToList();

            var user = people.FirstOrDefault(x => x is UserInheritanceTest);

            user.Should().Not.Be.Null();

            var getByDn = _context.GetByDN<PersonInheritanceTest>(user.DistinguishedName);

            getByDn.Should().Be.InstanceOf<UserInheritanceTest>();

            var utcNow = DateTime.UtcNow.AddDays(50);
            var lastName = utcNow.ToString();
            getByDn.LastName = lastName;
            getByDn.As<UserInheritanceTest>().AccountExpires = utcNow;

            var updated = _context.UpdateAndGet(getByDn);
            updated.Should().Be.InstanceOf<UserInheritanceTest>();
            updated.LastName.Should().Be.EqualTo(lastName);
            updated.As<UserInheritanceTest>().AccountExpires.GetValueOrDefault().ToUniversalTime().Should().Be.EqualTo(utcNow);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Test_Inheritance_CatchAll()
        {
            var people = _context.Query<PersonCatchAllTest>().ToList();

            people.Any(x => x is OrgPersonCatchAllTest).Should().Be.True();

            people.Last(x => !(x is OrgPersonCatchAllTest)).CommonName.Should().Not.Be.NullOrEmpty();
            people.Last(x => !(x is OrgPersonCatchAllTest)).CatchAll.GetString("sn").Should().Not.Be.NullOrEmpty();

            var utcNow = DateTime.UtcNow.AddDays(50);
            var user = people.OfType<OrgPersonCatchAllTest>().Last();
            user.CatchAll.GetString("l").Should().Not.Be.NullOrEmpty();
            user.Title.Should().Not.Be.NullOrEmpty();

            user.Title = utcNow.ToString("YY-MM-DD hhmm");
            user.CatchAll.Set("comment", utcNow.ToFileTime());
            var updatedUser = _context.UpdateAndGet(user);

            updatedUser.Should().Not.Be.Null();
            updatedUser.As<OrgPersonCatchAllTest>().Title.Should().Be.EqualTo(utcNow.ToString("YY-MM-DD hhmm"));
            updatedUser.As<OrgPersonCatchAllTest>().CatchAll.GetDateTime("comment", null).GetValueOrDefault().ToUniversalTime().Should().Be.EqualTo(utcNow);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Test_EventListener()
        {
            _configuration.RegisterListeners(this);

            var user = new IntegrationUserTest { Cn = "eventlistener", Mail = "mail@cool.com" };

            var added = _context.AddAndGet(user, user.GetDistinguishedName());
            try
            {
                added.Mail = "user@cool.com";
                _context.Update(added);
            }
            finally
            {
                _context.Delete(added.DistinguishedName);
            }

            _preAddNotified.Should().Be.True();
            _postAddNotified.Should().Be.True();

            _preUpdateNotified.Should().Be.True();
            _postUpdateNotified.Should().Be.True();

            _preDeleteNotified.Should().Be.True();
            _postDeleteNotified.Should().Be.True();
        }

        #region Async Tests

#if !NET35

        [TestMethod]
        [TestCategory("Integration")]
        public void Can_Query_Accross_Threads()
        {
            _context.FieldValueEx<LdapConnection>("_connection").Bind();
            Action work = () =>
            {
                for (int i = 0; i < 50; i++)
                {
                    _context.Query(IntegrationUserTest.NamingContext).ToList();
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
            var task = _context.Query<IntegrationGroupTest>()
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
            var task = _context.Query<IntegrationGroupTest>()
                .CountAsync();
            task.Wait();

            //assert
            task.Result.Should().Be.GreaterThan(1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void LongCountAsync_Executes()
        {
            //act
            var task = _context.Query<IntegrationGroupTest>()
                .LongCountAsync();
            task.Wait();

            //assert
            task.Result.Should().Be.GreaterThan(1);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void FirstOrDefaultAsync_Executes()
        {
            //act
            var task = _context.Query<IntegrationGroupTest>().FirstOrDefaultAsync();
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
            var task = _context.Query<IntegrationGroupTest>().FirstAsync();
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
            var task = _context.Query(IntegrationGroupTest.NamingContext, SearchScope.Base)
                .SingleAsync();
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(IntegrationGroupTest.NamingContext);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SingleAsync_WithPredicate_Executes()
        {
            //act
            var task = _context.Query(IntegrationGroupTest.NamingContext)
                .SingleAsync(g => Filter.Equal(g, "distinguishedName", IntegrationGroupTest.NamingContext, false));
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(IntegrationGroupTest.NamingContext);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SingleOrDefaultAsync_Executes()
        {
            //act
            var task = _context.Query(IntegrationGroupTest.NamingContext, SearchScope.Base)
                .SingleOrDefaultAsync();
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(IntegrationGroupTest.NamingContext);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void SingleOrDefaultAsync_WithPredicate_Executes()
        {
            //act
            var task = _context.Query(IntegrationGroupTest.NamingContext)
                .SingleOrDefaultAsync(g => Filter.Equal(g, "distinguishedName", IntegrationGroupTest.NamingContext, false));
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(IntegrationGroupTest.NamingContext);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ListServerAttributesAsync_Executes()
        {
            //act
            var task = _context.Query(IntegrationGroupTest.NamingContext, SearchScope.Base)
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
            var task = _context.Query(IntegrationGroupTest.NamingContext, SearchScope.Base)
                .ListAttributesAsync("cn");
            task.Wait();

            //assert
            task.Result.Should().Have.Count.EqualTo(1);
            task.Result.First().Value.Satisfies(kvp => kvp.Count() == 1 && kvp.First().Key == "cn");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetByDNAsync_MappedObject_Executes()
        {
            //act
            var task = _context.GetByDNAsync<IntegrationGroupTest>(IntegrationGroupTest.NamingContext);
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(IntegrationGroupTest.NamingContext);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetByDNAsync_UnmappedObject_Executes()
        {
            //act
            var task = _context.GetByDNAsync(IntegrationGroupTest.NamingContext);
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(IntegrationGroupTest.NamingContext);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetByDNAsync_LdapConnectionExtension_Executes()
        {
            //act
            var task = _context.FieldValueEx<LdapConnection>("_connection")
                .GetByDNAsync(IntegrationGroupTest.NamingContext);
            task.Wait();

            //assert
            task.Result.DistinguishedName.Should().Be.EqualTo(IntegrationGroupTest.NamingContext);
        }

#endif

        #endregion Async Tests

        private static byte[] GetPasswordData(string password)
        {
            var formattedPassword = string.Format("\"{0}\"", password);
            return (Encoding.Unicode.GetBytes(formattedPassword));
        }

        private bool _preAddNotified;

        public void Notify(ListenerPreArgs<object, AddRequest> preArgs)
        {
            preArgs.Entry.Should().Not.Be.Null();
            preArgs.Request.Should().Not.Be.Null();
            preArgs.Connection.Should().Not.Be.Null();

            _preAddNotified = true;
        }

        private bool _preUpdateNotified;

        public void Notify(ListenerPreArgs<object, ModifyRequest> preArgs)
        {
            preArgs.Entry.Should().Not.Be.Null();
            preArgs.Request.Should().Not.Be.Null();
            preArgs.Connection.Should().Not.Be.Null();

            _preUpdateNotified = true;
        }

        private bool _preDeleteNotified;

        public void Notify(ListenerPreArgs<string, DeleteRequest> preArgs)
        {
            preArgs.Entry.Should().Not.Be.Null();
            preArgs.Request.Should().Not.Be.Null();
            preArgs.Connection.Should().Not.Be.Null();

            preArgs.Request.Controls.Add(new TreeDeleteControl());

            _preDeleteNotified = true;
        }

        private bool _postAddNotified;

        public void Notify(ListenerPostArgs<object, AddRequest, AddResponse> postArgs)
        {
            postArgs.Entry.Should().Not.Be.Null();
            postArgs.Request.Should().Not.Be.Null();
            postArgs.Connection.Should().Not.Be.Null();
            postArgs.Response.Should().Not.Be.Null();
            postArgs.Response.ResultCode.Should().Be.EqualTo(ResultCode.Success);
            _postAddNotified = true;
        }

        private bool _postUpdateNotified;

        public void Notify(ListenerPostArgs<object, ModifyRequest, ModifyResponse> postArgs)
        {
            postArgs.Entry.Should().Not.Be.Null();
            postArgs.Request.Should().Not.Be.Null();
            postArgs.Response.Should().Not.Be.Null();
            postArgs.Response.ResultCode.Should().Be.EqualTo(ResultCode.Success);
            postArgs.Connection.Should().Not.Be.Null();
            _postUpdateNotified = true;
        }

        private bool _postDeleteNotified;

        public void Notify(ListenerPostArgs<string, DeleteRequest, DeleteResponse> postArgs)
        {
            postArgs.Entry.Should().Not.Be.Null();
            postArgs.Request.Should().Not.Be.Null();
            postArgs.Response.Should().Not.Be.Null();
            postArgs.Response.ResultCode.Should().Be.EqualTo(ResultCode.Success);
            postArgs.Connection.Should().Not.Be.Null();

            _postDeleteNotified = true;
        }
    }
}