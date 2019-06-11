using System.DirectoryServices.Protocols;
using System.Linq;
using System.Text;

namespace LinqToLdap.Logging
{
    internal static class LoggingExtensions
    {
        internal static string ToLogString(this DirectoryResponse response)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0} >", response.GetType().Name);
            sb.AppendLine();
            sb.AppendLine("ResultCode: " + response.ResultCode);
            sb.AppendLine("ErrorMessage: " + response.ErrorMessage);
            sb.AppendLine("MatchedDN: " + response.MatchedDN);
            sb.AppendLine("Referral: " + string.Join(", ", response.Referral.Select(u => u.ToString()).ToArray()));
            sb.AppendLine("RequestId: " + response.RequestId);
            sb.AppendLine("Controls: ");
            foreach (DirectoryControl control in response.Controls)
            {
                control.AppendTo(sb);
            }
            sb.AppendLine();
            return sb.ToString();
        }

        internal static string ToLogString(this SearchRequest searchRequest)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Search Request >");
            sb.AppendLine("Filter: " + searchRequest.Filter);
#if NET35
            sb.AppendLine("Attributes: " + string.Join(", ", searchRequest.Attributes.Cast<string>().ToArray()));
#else
            sb.AppendLine("Attributes: " + string.Join(", ", searchRequest.Attributes.Cast<string>()));
#endif
            sb.AppendLine("Naming Context: " + searchRequest.DistinguishedName);
            sb.AppendLine("Scope: " + searchRequest.Scope);
            sb.AppendLine("Types Only: " + searchRequest.TypesOnly);
            sb.AppendLine("Controls: ");
            foreach (DirectoryControl control in searchRequest.Controls)
            {
                control.AppendTo(sb);
            }
            sb.AppendLine();
            return sb.ToString();
        }

        internal static string ToLogString(this AddRequest addRequest)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Add Request >");
            sb.AppendLine("Distinguished Name: " + addRequest.DistinguishedName);
#if NET35
            sb.AppendLine("Attributes: " + string.Join(", ", addRequest.Attributes.Cast<DirectoryAttribute>().Select(FormatDirectoryAttribute).ToArray()));
#else
            sb.AppendLine("Attributes: " + string.Join(", ", addRequest.Attributes.Cast<DirectoryAttribute>().Select(FormatDirectoryAttribute)));
#endif
            sb.AppendLine("Request ID: " + addRequest.RequestId);
            sb.AppendLine("Controls: ");
            foreach (DirectoryControl control in addRequest.Controls)
            {
                control.AppendTo(sb);
            }

            return sb.ToString();
        }

        internal static string ToLogString(this DeleteRequest deleteRequest)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Delete Request >");
            sb.AppendLine("Distinguished Name: " + deleteRequest.DistinguishedName);
            sb.AppendLine("Request ID: " + deleteRequest.RequestId);
            sb.AppendLine("Controls: ");
            foreach (DirectoryControl control in deleteRequest.Controls)
            {
                control.AppendTo(sb);
            }

            return sb.ToString();
        }

        internal static string ToLogString(this ModifyRequest modifyRequest)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Modify Request >");
            sb.AppendLine("Distinguished Name: " + modifyRequest.DistinguishedName);
#if NET35
            sb.AppendLine("Attributes: " + string.Join(", ", modifyRequest.Modifications.Cast<DirectoryAttributeModification>().Select(FormatDirectoryAttribute).ToArray()));
#else
            sb.AppendLine("Attributes: " + string.Join(", ", modifyRequest.Modifications.Cast<DirectoryAttributeModification>().Select(FormatDirectoryAttribute)));
#endif
            sb.AppendLine("Request ID: " + modifyRequest.RequestId);
            sb.AppendLine("Controls: ");
            foreach (DirectoryControl control in modifyRequest.Controls)
            {
                control.AppendTo(sb);
            }

            return sb.ToString();
        }

        internal static string ToLogString(this ModifyDNRequest modifyDNRequest)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Modify DN Request >");
            sb.AppendLine("Current Distinguished Name: " + modifyDNRequest.DistinguishedName);
            sb.AppendLine("New Naming Context: " + modifyDNRequest.NewParentDistinguishedName);
            sb.AppendLine("New Common Name: " + modifyDNRequest.NewName);
            sb.AppendLine("Request ID: " + modifyDNRequest.RequestId);
            sb.AppendLine("Delete Old Rdn: " + modifyDNRequest.DeleteOldRdn);
            sb.AppendLine("Controls: ");
            foreach (DirectoryControl control in modifyDNRequest.Controls)
            {
                control.AppendTo(sb);
            }

            return sb.ToString();
        }

        internal static void AppendTo(this DirectoryControl control, StringBuilder sb)
        {
            if (control is PageResultRequestControl)
            {
                var pageControl = control as PageResultRequestControl;
                sb.AppendLine("Page Size: " + pageControl.PageSize);
                sb.AppendLine("Page Cookie Length: " + (pageControl.Cookie != null ? pageControl.Cookie.Length : 0));
                sb.AppendLine("Page Control Is Critical: " + control.IsCritical);
                sb.AppendLine("Page Control OID: " + control.Type);
            }
            if (control is VlvRequestControl)
            {
                var vlvControl = control as VlvRequestControl;
                sb.AppendLine("After Count: " + vlvControl.AfterCount);
                sb.AppendLine("Before Count: " + vlvControl.BeforeCount);
                sb.AppendLine("Offset: " + vlvControl.Offset);
                sb.AppendLine("Target: " + (vlvControl.Target == null || vlvControl.Target.Length == 0 ? "" : UTF8Encoding.UTF8.GetString(vlvControl.Target)));
                sb.AppendLine("Vlv Control Is Critical: " + control.IsCritical);
                sb.AppendLine("Vlv Control OID: " + control.Type);
            }
            else if (control is SortRequestControl)
            {
                var sortControl = control as SortRequestControl;
                var sortKey = sortControl.SortKeys.FirstOrDefault();
                if (sortKey != null)
                {
                    sb.AppendLine("Sort By: " + sortKey.AttributeName + " " + (sortKey.ReverseOrder ? "DESC" : "ASC"));
                }
                sb.AppendLine("Sort Control Is Critical: " + control.IsCritical);
                sb.AppendLine("Sort Control OID: " + control.Type);
            }
            else if (control is AsqRequestControl)
            {
                var asqControl = control as AsqRequestControl;
                sb.AppendLine("Attribute Scope: " + asqControl.AttributeName);
                sb.AppendLine("Attribute Scope Control Is Critical: " + control.IsCritical);
                sb.AppendLine("Attribute Scope Control OID: " + control.Type);
            }
            else
            {
                sb.AppendLine(control.GetType().Name + " Control");
                sb.AppendLine("Control Is Critical: " + control.IsCritical);
                sb.AppendLine("Control OID: " + control.Type);
            }
        }

        private const string AttributeFormat = "{{ {0}: {1} }}";

        private static string FormatDirectoryAttribute(DirectoryAttribute directoryAttribute)
        {
            if (directoryAttribute != null)
            {
                if (directoryAttribute.Count > 1)
                {
                    var type = directoryAttribute[0].GetType();

                    if (type == typeof(string))
                    {
#if NET35
                        return string.Format(AttributeFormat, directoryAttribute.Name,
                                             string.Join(" ", directoryAttribute.Cast<string>().ToArray()));
#else
                        return string.Format(AttributeFormat, directoryAttribute.Name,
                                             string.Join(" ", directoryAttribute.Cast<string>()));
#endif
                    }
                    if (type == typeof(byte[]))
                    {
#if NET35
                        return string.Format(AttributeFormat, directoryAttribute.Name,
                                             string.Join(" ", directoryAttribute.Cast<byte[]>().Select(b => b.ToStringOctet()).ToArray()));
#else
                        return string.Format(AttributeFormat, directoryAttribute.Name,
                                             string.Join(" ", directoryAttribute.Cast<byte[]>().Select(b => b.ToStringOctet())));
#endif
                    }
                }

                var value = directoryAttribute.Count == 1 ? directoryAttribute[0] : string.Empty;

                if (value is byte[])
                {
                    return string.Format(AttributeFormat, directoryAttribute.Name, (value as byte[]).ToStringOctet());
                }

                return string.Format(AttributeFormat, directoryAttribute.Name, value);
            }

            return string.Empty;
        }

        private static string FormatDirectoryAttribute(DirectoryAttributeModification directoryAttribute)
        {
            if (directoryAttribute != null)
            {
                string name = directoryAttribute.Name;
                string value = null;

                if (directoryAttribute.Count > 1)
                {
                    var type = directoryAttribute[0].GetType();

                    if (type == typeof(string))
                    {
#if NET35
                        value = string.Join(" ", directoryAttribute.Cast<string>().ToArray());
#else
                        value = string.Join(" ", directoryAttribute.Cast<string>());
#endif
                    }
                    else if (type == typeof(byte[]))
                    {
#if NET35
                        value = string.Join(" ", directoryAttribute.Cast<byte[]>().Select(b => b.ToStringOctet()).ToArray());
#else
                        value = string.Join(" ", directoryAttribute.Cast<byte[]>().Select(b => b.ToStringOctet()));
#endif
                    }
                }
                else if (directoryAttribute.Count > 0)
                {
                    var rawValue = directoryAttribute[0];
                    if (rawValue is byte[])
                    {
                        value = (rawValue as byte[]).ToStringOctet();
                    }
                    else
                    {
                        value = rawValue.ToString();
                    }
                }

                return directoryAttribute.Operation == DirectoryAttributeOperation.Delete
                    ? string.Format("{{ {0} {1} }}", "Delete", name)
                    : string.Format("{{ {0} {1}: {2} }}", directoryAttribute.Operation, name, value);
            }

            return string.Empty;
        }
    }
}