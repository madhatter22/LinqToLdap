using LinqToLdap.Collections;
using LinqToLdap.EventListeners;
using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using LinqToLdap.Tests.TestSupport.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.DirectoryServices.Protocols;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace LinqToLdap.Tests
{
    [DirectorySchema(NamingContext, ObjectClasses = new[] { "top", "person" })]
    public class PersonInheritanceTest : DirectoryObjectBase
    {
        public const string NamingContext = "CN=InheritanceTest,CN=Employees,DC=Northwind,DC=local";

        [DistinguishedName]
        public string DistinguishedName { get; set; }

        [DirectoryAttribute("sn")]
        public string LastName { get; set; }

        [DirectoryAttribute("whenchanged", true)]
        public DateTime? LastChanged { get; set; }

        [DirectoryAttribute("objectsid", true)]
        public SecurityIdentifier Sid { get; set; }

        [DirectoryAttribute("usnchanged", true)]
        public int Version { get; set; }

        [DirectoryAttribute("objectguid", true)]
        public Guid Guid { get; set; }

        [DirectoryAttribute("cn", true)]
        public string CommonName { get; set; }
    }

    [DirectorySchema(NamingContext, ObjectClasses = new[] { "top", "person", "organizationalPerson" })]
    public class OrgPersonInheritanceTest : PersonInheritanceTest
    {
        [DirectoryAttribute]
        public string Title { get; set; }

        [DirectoryAttribute]
        public string PostalCode { get; set; }

        [DirectoryAttribute(true)]
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

    [DirectorySchema(NamingContext, ObjectClasses = new[] { "top", "person", "organizationalPerson" }, WithoutSubTypeMapping = true)]
    public class OrgPersonFlattenedInheritanceTest : PersonInheritanceTest
    {
        [DirectoryAttribute]
        public string Title { get; set; }

        [DirectoryAttribute]
        public string PostalCode { get; set; }

        [DirectoryAttribute(true)]
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
        [DirectoryAttribute("badpwdcount", true)]
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

        [DirectoryAttribute("cn", true)]
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

        [DirectoryAttribute("cn", true)]
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
                .ReadOnly(ReadOnly.Always);

            Map(c => c.Mail);
            Map(c => c.ObjectGuid)
                .ReadOnly(ReadOnly.Always);
            Map(c => c.Manager)
                .Named("manager");
            Map(c => c.Employees)
                .Named("directreports")
                .ReadOnly(ReadOnly.Always);
            Map(c => c.AccountExpires)
                .Named("accountExpires")
                .DateTimeFormat(null)
                .DirectoryValue("9223372036854775807").Returns(null)
                .InstanceValue(DateTime.MinValue).Sends("9223372036854775807")
                .InstanceValueNullOrDefault().Sends("9223372036854775807");

            Map(c => c.WhenCreated)
                .ReadOnly(ReadOnly.Always);

            Map(c => c.ObjectSid)
                .ReadOnly(ReadOnly.Always);

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
                .AddMapping(new AttributeClassMap<OrgPersonFlattenedInheritanceTest>())
                .AddMapping(new AttributeClassMap<UserInheritanceTest>())
                .AddMapping(new AttributeClassMap<PersonCatchAllTest>())
                .AddMapping(new AttributeClassMap<OrgPersonCatchAllTest>())
                .MaxPageSizeIs(1000)
                .LogTo(new SimpleTextLogger(Console.Out));

            _configuration.ConfigureFactory(ServerName)
                //.AuthenticateBy(AuthType.Basic)
                //.AuthenticateAs(new NetworkCredential("CN=AlphaUser,CN=Users,CN=Employees,DC=Northwind,DC=local", "test"))
                .AuthenticateBy(AuthType.Ntlm)
                .ProtocolVersion(3);

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
            added.Should().NotBeNull();

            added.GetString("cn").Should().Be("IntegrationTest");
            added.GetString("accountexpires").Should().BeNull();
            added.GetGuid("objectguid").Should().HaveValue();
            added.GetSecurityIdentifier("objectsid").Should().NotBeNull();
            added.GetSecurityIdentifiers("objectsid").Should().NotBeEmpty();
            added.GetStrings("objectclass").Should().HaveCountGreaterThan(1);

            added.Set("accountExpires", "9223372036854775807").SetNull("manager");

            added = _context.UpdateAndGet(added);

            added.GetString("accountExpires").Should().Be("9223372036854775807");
            added.GetDateTime("accountExpires", null).Should().NotHaveValue();
            added.GetString("manager").Should().BeNull();

            var renamed = _context.RenameEntry(added.DistinguishedName, "IntegrationTest2");

            var moved = _context.MoveEntry(renamed, IntegrationUserTest.NamingContext2);

            _context.Delete(moved);

            Executing.This(() => _context.GetByDN(moved))
                .Should().Throw<DirectoryOperationException>().And.GetBaseException().Message
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

            added.Should().NotBeNull();
            added.AccountExpires.Should().NotHaveValue();
            added.Cn.Should().Be(test.Cn);
            added.AccountExpires.Should().NotHaveValue();
            added.ObjectGuid.Should().NotBe(null).And.NotBe(default(Guid));
            added.ObjectSid.Should().NotBeNull();
            added.Manager.Should().BeNull();
            added.Employees.Should().BeNull();
            added.WhenCreated.Should().BeOnOrAfter(DateTime.Now.Subtract(TimeSpan.FromHours(1)));

            var queryResult = _context.Query<IntegrationUserTest>()
                .Where(u => u.ObjectGuid == added.ObjectGuid)
                .Select(u => u.ObjectGuid)
                .FirstOrDefault();

            queryResult.Should().Be(added.ObjectGuid);

            var now = DateTime.Now;
            added.AccountExpires = now;

            added = _context.UpdateAndGet(added);

            added.AccountExpires.Should().Be(now);

            _context.Delete(added.DistinguishedName);

            Executing.This(() => _context.GetByDN(added.DistinguishedName))
                .Should().Throw<DirectoryOperationException>().And.GetBaseException().Message
                .Should().Contain("does not exist");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void GetByDN_LoadsObject()
        {
            _context.GetByDN("CN=TestUser,CN=Users2,CN=Employees,DC=Northwind,DC=local").Should().NotBeNull();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Can_Add_Update_ModifyMembers_Group()
        {
            var members = _context.Query<IntegrationUserTest>().Select(u => u.DistinguishedName).ToList();

            members.Count.Should().BeGreaterThan(0);
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

            added.OriginalValues["CommonName"].Should().Be("TestGroup");
            added.OriginalValues["Member"].CastTo<Collection<string>>().Should().HaveCount(members.Count - 1);
            added.OriginalValues["distinguishedname"].Should().Be(added.DistinguishedName);

            added.OriginalValues["CommonName"].Should().Be(addedQueried.OriginalValues["CommonName"]);
            added.OriginalValues["Member"].CastTo<Collection<string>>()
                .Should().ContainInOrder(addedQueried.OriginalValues["Member"].CastTo<Collection<string>>());
            added.OriginalValues["distinguishedname"].Should()
                .Be(addedQueried.OriginalValues["distinguishedname"]);

            foreach (var member in members.GetRange(0, members.Count - 1))
            {
                added.Member.Should().Contain(member);
            }

            int removedIndex = members.IndexOf(added.Member[0]);
            added.Member.Should().NotContain(members.Last());
            added.Member.RemoveAt(0);
            added.Member.Add(members.Last());

            var updated = _context.UpdateAndGet(added);

            updated.Should().NotBe(added);

            updated.Member.Count.Should().Be(members.Count - 1);
            updated.Member.Should().NotContain(members[removedIndex]);
            updated.Member.Should().Contain(members.Last());

            updated.OriginalValues["CommonName"].Should().Be("TestGroup");
            updated.OriginalValues["distinguishedname"].Should().Be(added.DistinguishedName);

            updated.OriginalValues["CommonName"].Should().Be(addedQueried.OriginalValues["CommonName"]);
            updated.OriginalValues["Member"].CastTo<Collection<string>>()
                .Should().ContainInOrder(updated.Member);
            updated.OriginalValues["distinguishedname"].Should()
                .Be(addedQueried.OriginalValues["distinguishedname"]);

            _context.Delete(updated.DistinguishedName);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Can_Modify_Individual_Members_Group()
        {
            var members = _context.Query<IntegrationUserTest>().Select(u => u.DistinguishedName).ToList();

            members.Count.Should().BeGreaterThan(2);
            var group = new IntegrationGroupTest
            {
                Member = new Collection<string>(members.Skip(1).ToArray())
            };

            group.SetDistinguishedName("TestGroup");

            try
            {
                var added = _context.AddAndGet(group);

                added.Member.Should().NotContain(members.First()).And.HaveCount(members.Count - 1);

                _context.AddAttribute(group.DistinguishedName, "member", members.First());

                added = _context.GetByDN<IntegrationGroupTest>(group.DistinguishedName);

                added.Member.Should().Contain(members.First()).And.HaveCount(members.Count);

                _context.DeleteAttribute(group.DistinguishedName, "member", members.First());

                added = _context.GetByDN<IntegrationGroupTest>(group.DistinguishedName);

                added.Member.Should().NotContain(members.First()).And.HaveCount(members.Count - 1);
            }
            finally
            {
                try
                {
                    _context.Delete(group.DistinguishedName);
                }
                finally
                {
                }
            }
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Can_Add_Update_ModifyMembersDynamic_Group()
        {
            var members = _context.Query<IntegrationUserTest>().Select(u => u.DistinguishedName).ToList();
            members.Count.Should().BeGreaterThan(0);

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
            addedMembers.Should().NotContain(members.Last());
            addedMembers.RemoveAt(0);
            addedMembers.Add(members.Last());

            added.Set("member", addedMembers);

            var updated = _context.UpdateAndGet(added);

            updated.GetStrings("member").Should().NotContain(members[removedIndex])
                .And.Contain(members.Last());

            _context.Delete(updated.DistinguishedName);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void RenameEntry_Container_RenamesEntryAndAllEntriesWithin()
        {
            string dn = "CN=Users2,CN=Employees,DC=Northwind,DC=local";
            var countResult = _context.Query(dn).Count(da => !Filter.Equal(da, "objectClass", "container", false));
            countResult.Should().BeGreaterThanOrEqualTo(4);
            var newDn = _context.RenameEntry(dn, "Users Rename");
            _context.Query(newDn).Count(da => !Filter.Equal(da, "objectClass", "container", false)).Should().BeGreaterThanOrEqualTo(4);
            _context.RenameEntry(newDn, "Users2");
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ListServerAttributes()
        {
            var attributes = _context.ListServerAttributes();
            attributes.Should().NotBeNull().And.NotBeEmpty();
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

            response2.Count.Should().Be(2);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void ToArray_Works()
        {
            _context.Query<IntegrationUserTest>().ToArray().Should().NotBeNull().And.NotBeEmpty();
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
                .Should().Throw<DirectoryOperationException>().And.GetBaseException().Message
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
            group.Should().HaveCountGreaterThan(2000);
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
            group.Should().HaveCountGreaterThan(5000).And.HaveCountLessThan(10000);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void WithoutPaging_LargeResultSet_ThrowsException()
        {
            //act
            Executing.This(() => _context.Query("CN=Users,CN=Employees,DC=Northwind,DC=local").WithoutPaging().ToList())
                .Should().Throw<DirectoryOperationException>().And.GetBaseException().Message
                .Should().Be("The size limit was exceeded");
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

                users.Should().NotBeEmpty();
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

            list.Should().HaveCountGreaterThan(1000);
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
            readerGroup.Should().NotBeNull();
            readerGroup.Member.Should().BeNull();
            var user = _context.Query<IntegrationUserTest>().FirstOrDefault();
            user.Should().NotBeNull();
            readerGroup.Member = new Collection<string>(new List<string> { user.DistinguishedName });

            //act
            var updated = _context.UpdateAndGet(readerGroup);

            //assert
            updated.Should().NotBeSameAs(readerGroup);
            updated.Member.Should().Contain(user.DistinguishedName);

            //prepare
            updated.Member.Clear();

            //act
            var updatedAgain = _context.UpdateAndGet(updated);

            //assert
            updatedAgain.Should().NotBeSameAs(updated);
            updatedAgain.Member.Should().BeNull();
        }

        [TestMethod]
        [TestCategory("Integration")]
        [Ignore]//Control isn't supported by Lightweight Directory Services
        public void Virtual_List_View_Test()
        {
            int skip = 0;
            int take = 700;
            string[] attribs = { "cn", "sn", "givenName" };

            var results = _context.Query(IntegrationUserTest.NamingContext2, SearchScope.Subtree)
                .Where(x => Filter.StartsWith(x, "givenName", "J", true))
                .OrderByDescending("cn")
                .Select(attribs)
                .Skip(skip)
                .Take(700);

            results.Should().NotBeEmpty().And.HaveCount(700);
            results.CastTo<IVirtualListView<IntegrationUserTest>>().Should().NotBeNull();
            results.CastTo<IVirtualListView<IntegrationUserTest>>().ContextId
                .Should().NotBeNull();
            results.CastTo<IVirtualListView<IntegrationUserTest>>().TargetPosition
                .Should().BeGreaterThanOrEqualTo(0);

            SearchRequest searchRequest = new SearchRequest
                                                    (IntegrationUserTest.NamingContext2,
                                                     "(givenName=J*)",
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

            searchResponse.Should().NotBeNull();
            searchResponse.Entries.Count.Should().BeGreaterThan(0);
            results.CastTo<IVirtualListView<IntegrationUserTest>>().ContentCount
                .Should().Be(searchResponse.Controls[1].CastTo<VlvResponseControl>().ContentCount);

            results.Select(x => x.DistinguishedName)
                .OrderBy(x => x)
                .Should()
                .ContainInOrder(
                    searchResponse.Entries.Cast<SearchResultEntry>().Select(x => x.DistinguishedName).OrderBy(x => x));
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Test_Inheritance()
        {
            var people = _context.Query<PersonInheritanceTest>().ToList();

            people.Any(x => x is OrgPersonInheritanceTest).Should().BeTrue();
            people.Any(x => x is OrgPersonFlattenedInheritanceTest).Should().BeFalse();
            people.Any(x => x is UserInheritanceTest).Should().BeTrue();

            _context.Query<OrgPersonFlattenedInheritanceTest>().ToList().Should().NotBeEmpty();

            var utcNow = DateTime.UtcNow.AddDays(50);
            var user = people.OfType<UserInheritanceTest>().Skip(1).First();
            user.City.Should().NotBeNullOrEmpty();
            user.Title.Should().NotBeNullOrEmpty();
            user.AccountExpires.Should().BeBefore(utcNow);
            user.AccountExpires = utcNow;
            user.WhenCreated.Should().HaveValue();
            _context.Update(user);

            var updatedUser =
                _context.Query<PersonInheritanceTest>().FirstOrDefault(x => x.CommonName == user.CommonName);

            updatedUser.Should().NotBeNull();
            updatedUser.CastTo<UserInheritanceTest>().AccountExpires.Should().HaveValue();
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Test_Inheritance_GetByDn_And_Update()
        {
            var people = _context.Query<PersonInheritanceTest>().Take(100).ToList();

            var user = people.FirstOrDefault(x => x is UserInheritanceTest);

            user.Should().NotBeNull();

            var getByDn = _context.GetByDN<PersonInheritanceTest>(user.DistinguishedName);

            getByDn.Should().BeOfType<UserInheritanceTest>();

            var utcNow = DateTime.UtcNow.AddDays(50);
            var lastName = utcNow.ToString();
            getByDn.LastName = lastName;
            getByDn.CastTo<UserInheritanceTest>().AccountExpires = utcNow;

            var updated = _context.UpdateAndGet(getByDn);
            updated.Should().BeOfType<UserInheritanceTest>();
            updated.LastName.Should().Be(lastName);
            updated.CastTo<UserInheritanceTest>().AccountExpires.GetValueOrDefault().ToUniversalTime().Should().Be(utcNow);
        }

        [TestMethod]
        [TestCategory("Integration")]
        public void Test_Inheritance_CatchAll()
        {
            var people = _context.Query<PersonCatchAllTest>().ToList();

            people.Any(x => x is OrgPersonCatchAllTest).Should().BeTrue();

            people.Last(x => !(x is OrgPersonCatchAllTest)).CommonName.Should().NotBeNullOrEmpty();
            people.Last(x => !(x is OrgPersonCatchAllTest)).CatchAll.GetString("sn").Should().NotBeNullOrEmpty();

            var utcNow = DateTime.UtcNow.AddDays(50);
            var user = people.OfType<OrgPersonCatchAllTest>().Last();
            user.CatchAll.GetString("l").Should().NotBeNullOrEmpty();
            user.Title.Should().NotBeNullOrEmpty();

            user.Title = utcNow.ToString("YY-MM-DD hhmm");
            user.CatchAll.Set("comment", utcNow.ToFileTime());
            var updatedUser = _context.UpdateAndGet(user);

            updatedUser.Should().NotBeNull();
            updatedUser.CastTo<OrgPersonCatchAllTest>().Title.Should().Be(utcNow.ToString("YY-MM-DD hhmm"));
            updatedUser.CastTo<OrgPersonCatchAllTest>().CatchAll.GetDateTime("comment", null).GetValueOrDefault().ToUniversalTime().Should().Be(utcNow);
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

            _preAddNotified.Should().BeTrue();
            _postAddNotified.Should().BeTrue();

            _preUpdateNotified.Should().BeTrue();
            _postUpdateNotified.Should().BeTrue();

            _preDeleteNotified.Should().BeTrue();
            _postDeleteNotified.Should().BeTrue();
        }

#if !NET35

        [TestMethod]
        [TestCategory("Integration")]
        public void Test_Pooledfactory()
        {
            var configuration = new LdapConfiguration()
                .AddMapping(new IntegrationUserTestMapping(), IntegrationUserTest.NamingContext, new[] { "user" })
                .AddMapping(new AttributeClassMap<IntegrationGroupTest>(), IntegrationGroupTest.NamingContext, new[] { "top", "group" }, true, "group")
                .AddMapping(new AttributeClassMap<PersonInheritanceTest>())
                .AddMapping(new AttributeClassMap<OrgPersonInheritanceTest>())
                .AddMapping(new AttributeClassMap<UserInheritanceTest>())
                .AddMapping(new AttributeClassMap<PersonCatchAllTest>())
                .AddMapping(new AttributeClassMap<OrgPersonCatchAllTest>())
                .MaxPageSizeIs(1000)
                .LogTo(new SimpleTextLogger(Console.Out));

            configuration.ConfigurePooledFactory(ServerName)
                .AuthenticateBy(AuthType.Ntlm)
                .ProtocolVersion(3);

            System.Threading.Tasks.Parallel.For(0, 10, new System.Threading.Tasks.ParallelOptions { MaxDegreeOfParallelism = 4 }, x =>
            {
                using (var context = configuration.CreateContext())
                {
                    var list = context.Query<IntegrationUserTest>().Where(u => u.Mail == "test").ToList();
                }
            });
        }

#endif

        private static byte[] GetPasswordData(string password)
        {
            var formattedPassword = string.Format("\"{0}\"", password);
            return (Encoding.Unicode.GetBytes(formattedPassword));
        }

        private bool _preAddNotified;

        public void Notify(ListenerPreArgs<object, AddRequest> preArgs)
        {
            preArgs.Entry.Should().NotBeNull();
            preArgs.Request.Should().NotBeNull();
            preArgs.Connection.Should().NotBeNull();

            _preAddNotified = true;
        }

        private bool _preUpdateNotified;

        public void Notify(ListenerPreArgs<object, ModifyRequest> preArgs)
        {
            preArgs.Entry.Should().NotBeNull();
            preArgs.Request.Should().NotBeNull();
            preArgs.Connection.Should().NotBeNull();

            _preUpdateNotified = true;
        }

        private bool _preDeleteNotified;

        public void Notify(ListenerPreArgs<string, DeleteRequest> preArgs)
        {
            preArgs.Entry.Should().NotBeNull();
            preArgs.Request.Should().NotBeNull();
            preArgs.Connection.Should().NotBeNull();

            preArgs.Request.Controls.Add(new TreeDeleteControl());

            _preDeleteNotified = true;
        }

        private bool _postAddNotified;

        public void Notify(ListenerPostArgs<object, AddRequest, AddResponse> postArgs)
        {
            postArgs.Entry.Should().NotBeNull();
            postArgs.Request.Should().NotBeNull();
            postArgs.Connection.Should().NotBeNull();
            postArgs.Response.Should().NotBeNull();
            postArgs.Response.ResultCode.Should().Be(ResultCode.Success);
            _postAddNotified = true;
        }

        private bool _postUpdateNotified;

        public void Notify(ListenerPostArgs<object, ModifyRequest, ModifyResponse> postArgs)
        {
            postArgs.Entry.Should().NotBeNull();
            postArgs.Request.Should().NotBeNull();
            postArgs.Response.Should().NotBeNull();
            postArgs.Response.ResultCode.Should().Be(ResultCode.Success);
            postArgs.Connection.Should().NotBeNull();
            _postUpdateNotified = true;
        }

        private bool _postDeleteNotified;

        public void Notify(ListenerPostArgs<string, DeleteRequest, DeleteResponse> postArgs)
        {
            postArgs.Entry.Should().NotBeNull();
            postArgs.Request.Should().NotBeNull();
            postArgs.Response.Should().NotBeNull();
            postArgs.Response.ResultCode.Should().Be(ResultCode.Success);
            postArgs.Connection.Should().NotBeNull();

            _postDeleteNotified = true;
        }
    }
}