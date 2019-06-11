using LinqToLdap.Logging;
using LinqToLdap.Mapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace LinqToLdap
{
    ///<summary>
    /// Class containing useful extension methods.
    ///</summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Converts a <see cref="Guid"/> to a string octect.
        /// </summary>
        /// <param name="guid">Original <see cref="Guid"/></param>
        /// <returns></returns>
        public static string ToStringOctet(this Guid guid)
        {
            return guid.ToByteArray().ToStringOctet();
        }

        /// <summary>
        /// Converts a <see cref="Guid"/> to a string octect.
        /// </summary>
        /// <param name="bytes">Original <see cref="byte"/> array</param>
        /// <returns></returns>
        public static string ToStringOctet(this byte[] bytes)
        {
#if NET35
            return @"\" + string.Join(@"\", bytes.Select(b => b.ToString("x2")).ToArray());
#else
            return @"\" + string.Join(@"\", bytes.Select(b => b.ToString("x2")));
#endif
        }

        #region DateTime Extensions

        internal const string LdapFormat = "yyyyMMddHHmmss.0Z";

        internal static DateTime FormatLdapDateTime(this object obj, string format)
        {
            var value = DateTimeOffset.ParseExact(obj.ToString(), format, DateTimeFormatInfo.InvariantInfo).DateTime;
            return value;
        }

        /// <summary>
        /// Converts a date time to a string..
        /// </summary>
        /// <param name="dateTime">The original date</param>
        /// <param name="format">The format of the date</param>
        /// <example>
        /// yyyyMMddHHmmss.0Z
        /// </example>
        /// <exception cref="FormatException">
        /// </exception>
        /// <returns></returns>
        public static string FormatLdapDateTime(this DateTime dateTime, string format)
        {
            var value = dateTime.ToString(format, DateTimeFormatInfo.InvariantInfo);
            return value;
        }

        #endregion DateTime Extensions

#if (!NET35 && !NET40)

        /// <summary>
        /// Converts a dictionary to a <see cref="System.Collections.ObjectModel.ReadOnlyDictionary{K,V}"/>
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="dictionary">The original dictionary</param>
        /// <returns></returns>
        public static System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return new System.Collections.ObjectModel.ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

#else
        /// <summary>
        /// Converts a dictionary to a <see cref="LinqToLdap.Collections.ReadOnlyDictionary{K,V}"/>
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="dictionary">The original dictionary</param>
        /// <returns></returns>
        public static LinqToLdap.Collections.ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            return new LinqToLdap.Collections.ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

        /// <summary>
        /// Converts a dictionary to a <see cref="LinqToLdap.Collections.ReadOnlyDictionary{K,V}"/>
        /// </summary>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <typeparam name="TValue">Value type</typeparam>
        /// <param name="dictionary">The original dictionary</param>
        /// <returns></returns>
        public static LinqToLdap.Collections.ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
        {
            return new LinqToLdap.Collections.ReadOnlyDictionary<TKey, TValue>(dictionary);
        }

#endif

        /// <summary>
        /// Indicates if the <paramref name="type"/> is for an anonymous type.
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns></returns>
        public static bool IsAnonymous(this Type type)
        {
            var isAnonymousType = type.Name.Contains("AnonymousType") &&
                type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any() &&
                type.IsSealed;

            return isAnonymousType;
        }

        internal static bool HasDirectorySchema(this Type type)
        {
            var attributes = type.GetCustomAttributes(typeof(DirectorySchemaAttribute), true);
            return attributes != null && attributes.Length > 0;
        }

        internal static bool IsNullOrEmpty(this String str)
        {
#if NET35
            return string.IsNullOrEmpty(str) || string.IsNullOrEmpty(str.Trim());
#else
            return string.IsNullOrWhiteSpace(str);
#endif
        }

        internal static void AssertSuccess(this DirectoryResponse response)
        {
            if (response == null)
            {
                throw new LdapException("Null response returned from server.");
            }
            if (response.ResultCode != ResultCode.Success)
            {
                throw new LdapException(response.ToLogString());
            }
        }

        /// <summary>
        /// Cleans special characters for an LDAP filter.  This method cannot clean a distinguished name.
        /// </summary>
        /// <param name="value">The value to clean</param>
        /// <returns></returns>
        public static string CleanFilterValue(this string value)
        {
            var sb = new StringBuilder();
            foreach (var curChar in value)
            {
                switch (curChar)
                {
                    case '\\':
                        sb.Append("\\5c");
                        break;

                    case '*':
                        sb.Append("\\2a");
                        break;

                    case '(':
                        sb.Append("\\28");
                        break;

                    case ')':
                        sb.Append("\\29");
                        break;

                    case '&':
                        sb.Append("\\26");
                        break;

                    case ':':
                        sb.Append("\\3a");
                        break;

                    case '|':
                        sb.Append("\\7c");
                        break;

                    case '~':
                        sb.Append("\\7e");
                        break;

                    case '!':
                        sb.Append("\\21");
                        break;

                    case '\u0000':
                        sb.Append("\\00");
                        break;

                    default:
                        sb.Append(curChar);
                        break;
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Attempts to convert the object from a .Net type to an LDAP string or byte[].
        /// If <paramref name="obj"/> is null or <see cref="String.Empty"/> then no value is added to the <see cref="DirectoryAttributeModification"/>.
        /// </summary>
        /// <param name="obj">The value to convert.</param>
        /// <param name="attributeName">The name of the attribute.</param>
        /// <param name="operation">The type of <see cref="DirectoryAttributeOperation"/>.</param>
        /// <returns></returns>
        public static DirectoryAttributeModification ToDirectoryModification(this object obj, string attributeName, DirectoryAttributeOperation operation)
        {
            var modification = new DirectoryAttributeModification { Name = attributeName, Operation = operation };

            if (obj == null || string.Empty.Equals(obj)) return modification;

            if (obj is string)
            {
                modification.Add(obj as string);
                return modification;
            }

            if (obj is IEnumerable<string>)
            {
                foreach (var s in obj as IEnumerable<string>)
                {
                    modification.Add(s);
                }

                return modification;
            }
            if (obj is byte[])
            {
                modification.Add(obj as byte[]);
                return modification;
            }
            if (obj is X509Certificate)
            {
                modification.Add((obj as X509Certificate).GetRawCertData());
                return modification;
            }
            if (obj is IEnumerable<byte>)
            {
                modification.Add((obj as IEnumerable<byte>).ToArray());
                return modification;
            }
            if (obj is SecurityIdentifier)
            {
                var sid = obj as SecurityIdentifier;
                var bytes = new byte[sid.BinaryLength];
                sid.GetBinaryForm(bytes, 0);
                modification.Add(bytes);
                return modification;
            }
            if (obj is IEnumerable<byte[]>)
            {
                foreach (var b in (obj as IEnumerable<byte[]>).Where(b => b != null))
                {
                    modification.Add(b);
                }
                return modification;
            }
            if (obj is IEnumerable<X509Certificate>)
            {
                foreach (var b in (obj as IEnumerable<X509Certificate>).Where(c => c != null))
                {
                    modification.Add(b.GetRawCertData());
                }
                return modification;
            }
            if (obj is IEnumerable)
            {
                foreach (var s in (from object item in (obj as IEnumerable) select item.ToString()))
                {
                    modification.Add(s);
                }
                return modification;
            }
            if (obj is Guid)
            {
                modification.Add(((Guid)obj).ToByteArray());
                return modification;
            }
            if (obj is bool boolean)
            {
                modification.Add(boolean ? "TRUE" : "FALSE");

                return modification;
            }

            modification.Add(obj.ToString());

            return modification;
        }

        internal static DirectoryAttribute ToDirectoryAttribute(this object obj, string attributeName)
        {
            return ToDirectoryModification(obj, attributeName, DirectoryAttributeOperation.Replace);
        }

        internal static IEnumerable<SearchResultEntry> GetRange(this SearchResultEntryCollection collection)
        {
            for (int i = 0; i < collection.Count; i++)
            {
                yield return collection[i];
            }
        }
    }
}