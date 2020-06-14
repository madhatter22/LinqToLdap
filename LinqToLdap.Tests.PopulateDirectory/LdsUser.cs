using LinqToLdap.Mapping;
using System;
using System.Collections.ObjectModel;
using System.Security.Principal;

namespace LinqToLdap.Tests.PopulateDirectory
{
    [DirectorySchema("", ObjectClass = "User")]
    public class LdsUser

    {
        [DistinguishedName]
        public string DistinguishedName { get; set; }

        [DirectoryAttribute]
        public string Manager { get; set; }

        [DirectoryAttribute("badpwdcount", true)]
        public int BadPasswordCount { get; set; }

        public Collection<LdsUser> Employees { get; set; }

        [DirectoryAttribute]
        public string Title { get; set; }

        [DirectoryAttribute]
        public string PostalCode { get; set; }

        [DirectoryAttribute("cn", true)]
        public string CommonName { get; set; }

        [DirectoryAttribute(true)]
        public DateTime? WhenCreated { get; set; }

        [DirectoryAttribute("givenname")]
        public string FirstName { get; set; }

        [DirectoryAttribute("objectguid", true)]
        public Guid Guid { get; set; }

        [DirectoryAttribute("l")]
        public string City { get; set; }

        [DirectoryAttribute("usnchanged", true)]
        public int Version { get; set; }

        [DirectoryAttribute("c")]
        public string Country { get; set; }

        [DirectoryAttribute("whenchanged", true)]
        public DateTime? LastChanged { get; set; }

        [DirectoryAttribute("objectsid", true)]
        public SecurityIdentifier Sid { get; set; }

        [DirectoryAttribute("employeeid")]
        public long EmployeeId { get; set; }

        [DirectoryAttribute("telephonenumber")]
        public string PhoneNumber { get; set; }

        [DirectoryAttribute]
        public string Street { get; set; }

        [DirectoryAttribute]
        public string Comment { get; set; }

        [DirectoryAttribute]
        public string Name { get; set; }

        [DirectoryAttribute("sn")]
        public string LastName { get; set; }

        [DirectoryAttribute("notes")]
        public string Comments { get; set; }
    }
}