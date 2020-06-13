LINQ provider built on top of System.DirectoryServices.Protocols for interacting with LDAP servers.

## Overview
There are tons of examples on how to access Active Directory via System.DirectoryServices and System.DirectoryServices.AccountManagement.  However, with the introduction of LINQ all of those examples felt completely outdated.  The goal of LINQ to LDAP is to simplify the process of getting information in an out of a directory server using LINQ.

## Getting Started
Below you'll find the fastest way to get up and running with LINQ to LDAP.  There's also an examples project [here](https://github.com/madhatter22/linqtoldap_examples) with a MVC and WPF example that connects to a live OpenLDAP server.  You can also reference the [Documentation](https://github.com/madhatter22/LinqToLdap/wiki).

#### Get LINQ to LDAP
Add LINQ to LDAP to your project either by getting it from [NuGet](http://nuget.org/List/Packages/linqtoldap).
#### Create a class
```
public class User
{
    public const string NamingContext = "CN=Users,CN=Employees,DC=Northwind,DC=local";

    public string DistinguishedName { get; set; }
    public string CommonName { get; set; }
    public Guid Guid { get; set; }
    public SecurityIdentifier Sid { get; set; }
    public string Title { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime WhenCreated { get; set; }
    public DateTime LastChanged { get; set; }

    public void SetDistinguishedName()
    {
        DistinguishedName = string.Format("CN={0},{1}", CommonName, NamingContext);
    }
}
```
#### Map the class
By Attributes
```
[DirectorySchema(NamingContext, ObjectCategory = "Person", ObjectClass = "User")]
public class User
{
    public const string NamingContext = "CN=Users,CN=Employees,DC=Northwind,DC=local";

    [DistinguishedName](DistinguishedName)
    public string DistinguishedName { get; set; }

    [DirectoryAttribute("cn", ReadOnly = true)]
    public string CommonName { get; set; }

    [DirectoryAttribute("objectguid", StoreGenerated = true)]
    public Guid Guid { get; set; }

    [DirectoryAttribute("objectsid", StoreGenerated = true)]
    public SecurityIdentifier Sid { get; set; }

    [DirectoryAttribute](DirectoryAttribute)
    public string Title { get; set; }

    [DirectoryAttribute("givenname")]
    public string FirstName { get; set; }

    [DirectoryAttribute("sn")]
    public string LastName { get; set; }

    [DirectoryAttribute(StoreGenerated = true)]
    public DateTime WhenCreated { get; set; }

    [DirectoryAttribute(StoreGenerated = true)]
    public DateTime WhenChanged { get; set; }

    public void SetDistinguishedName()
    {
        DistinguishedName = string.Format("CN={0},{1}", CommonName, NamingContext);
    }
}
```
By Mapping
```
public class UserMapping : ClassMap<User>
{
    public override IClassMap PerformMapping(string namingContext = null, 
        string objectCategory = null, bool includeObjectCategory = true,
        IEnumerable<string> objectClasses = null, bool includeObjectClasses = true)
    {
        NamingContext(User.NamingContext);

        ObjectCategory("Person");
        ObjectClass("User");

        DistinguishedName(x => x.DistinguishedName);
        Map(x => x.CommonName).Named("cn").ReadOnly();
        Map(x => x.Guid).Named("objectguid").StoreGenerated();
        Map(x => x.Sid).Named("objectsid").StoreGenerated();
        Map(x => x.Title);
        Map(x => x.FirstName).Named("givenname");
        Map(x => x.LastName).Named("sn");
        Map(x => x.WhenCreated).StoreGenerated();
        Map(x => x.WhenChanged).StoreGenerated();

        return this;
    }
}
```
#### Configure LINQ to LDAP
Put this in your Startup.cs, Global.asax.cs or App.xaml.cs
```
var config = new LdapConfiguration()
    .MaxPageSizeIs(1000);

//add mapping
config.AddMapping(new UserMapping());
// OR
config.AddMapping(new AttributeClassMap<User>());

//configure connecting to the directory
config.ConfigureFactory("companydirectory.com")
    .AuthenticateBy(AuthType.Negotiate)
    .AuthenticateAs(CredentialCache.DefaultNetworkCredentials)
    .ProtocolVersion(3)
    .UsePort(389);

//store the configuration
//alternately you can register it with your IoC container of choice.
config.UseStaticStorage();
```
#### Create a DirectoryContext
```
//this only works when using static storage
IDirectoryContext context = new DirectoryContext();

//create it from a configuration
IDirectoryContext context = new DirectoryContext(config);
```
#### CRUD for User
```
IDirectoryContext context = new DirectoryContext();

var user = new User()
{
    CommonName = "ABC User",
    FirstName = "ABC",
    LastName = "User",
    Title = "Test User"
};
user.SetDistinguishedName();

context.Add(user);

user = context.Query<User>()
    .Single(u => u.FirstName == user.FirstName && u.LastName == user.LastName);

user.Should().Not.Be.Null();
user.Title = "Freshly updated";
context.Update(user);
context.Delete(user.DistinguishedName);
```
#### CRUD without mapping
```
IDirectoryContext context = new DirectoryContext();

IDirectoryAttributes user = new DirectoryAttributes("CN=ABC User,CN=Users,CN=Employees,DC=Northwind,DC=local");

user.Set("givenname", "ABC")
    .Set("sn", "User")
    .Set("title", "Test User");

context.Add(user);

user = context.Query("CN=Users,CN=Employees,DC=Northwind,DC=local")
    .Select("title", "cn", "givenname", "sn", "objectGuid", "objectSid")
    .Single(_ => Filter.Equal(_, "givenname", "ABC", shouldCleanValue: true) &&
                    Filter.Equal(_, "sn", "User", shouldCleanValue: true));

user.Should().Not.Be.Null();
user.GetGuid("objectGuid").Should().Not.Be.Null();
user.Set("title", "Freshly updated");
context.Update(user);
context.Delete(user.DistinguishedName);
```
## Lifetime Management
The LdapConfiguration class is designed to be a singleton.  You should configure it once and then store it with your IoC container of choice or call the UseStaticStorage method.  I don't recommend using DirectoryContext as a singleton, but it is possible depending on your usage scenario.  There may be limitations to how long a connection to your LDAP server can be maintained.
#### Thread-safety
LdapConfiguration is only thread safe after it has been configured.  DirectoryContext can be used across threads, but the underlying LdapConnection is [not guaranteed to be thread safe](http://msdn.microsoft.com/en-us/library/system.directoryservices.protocols.ldapconnection%28v=vs.110%29.aspx) so proceed with caution.
#### Disposal
Calling dispose is not explicitly required for DirectoryContext.  Both it and the LdapConnection from S.DS.P have a finalizer that will get called when the garbage collector runs.  However, depending on your connection factory and usage scenario, you may run out of connections to your LDAP server.
#### Unit-of-work
LDAP currently does **NOT** support transactions so there is no way to wrap modifications in a unit-of-work.
## Unit Testing
DirectoryContext is mocking friendly via the IDirectoryContext interface.  Its methods can be mocked, however, when using LINQ to LDAP specific expressions in queries (custom filters) it will be necessary to use the MockQuery under TestSupport.

```
var array = new[]() { "one" };
var context = new Mock<IDirectoryContext>();
var query = new MockQuery<IDirectoryAttributes>(new List<object> { array });
context.Setup(x => x.Query("test", SearchScope.Subtree, null, null, null))
    .Returns(query);
var expression = PredicateBuilder.Create<IDirectoryAttributes>()
    .And(x => Filter.Equal(x, "x", "y", false))
    .Or(x => Filter.Equal(x, "a", "b", true));

var result = context.Object.Query("test")
    .Where(expression)
    .Select(x => x.GetString("whatever"))
    .ToArray();

query.MockProvider.ExecutedExpressions.Should().Have.Count.EqualTo(1);
query.MockProvider.ExecutedExpressions[0].ToString()
    .Should().Contain("Equal(x, \"x\", \"y\", False)")
    .And.Contain("OrElse")
    .And.Contain("Equal(x, \"a\", \"b\", True)")
    .And.Contain("x => x.GetString(\"whatever\")");

result.Should().Have.SameSequenceAs(array);
```
## Tested Servers
* Microsoft Lightweight Directory Services
* Microsoft Active Directory (Read Only Testing)
* OpenLDAP (Read Only Testing)
* IBM Tivoli Directory (Read Only Testing)
* Siemens DirX (Read Only Testing)
* Apache Directory (Read Only Testing)
* OpenDJ (User Tested)

## Disclaimer
This project is provided as is.  I have tested the project primarily using Lightweight Directory Services, but each server will have different features and capabilities.  I recommend thorough testing before using this project in a production environment.
